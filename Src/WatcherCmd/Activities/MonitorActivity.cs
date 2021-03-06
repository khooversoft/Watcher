﻿using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherCmd.Application;
using WatcherSdk.Records;
using Toolbox.Extensions;

namespace WatcherCmd.Activities
{
    internal class MonitorActivity
    {
        private readonly IOption _option;
        private readonly ICosmosWatcherOption _watcherOption;
        private readonly ILogger<MonitorActivity> _logger;

        public MonitorActivity(IOption option, ICosmosWatcherOption watcherOption, ILogger<MonitorActivity> logger)
        {
            _option = option;
            _watcherOption = watcherOption;
            _logger = logger;
        }

        public Task Run(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Run)}: Starting monitoring");

            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            Console.CancelKeyPress += (sender, args) =>
            {
                _logger.LogInformation($"{nameof(Run)}: Canceling monitoring");
                args.Cancel = true;
                tokenSource.Cancel();
            };

            var cts = new TaskCompletionSource<bool>();

            _ = Task.Run(async () =>
            {
                CosmosClient cosmosClient = new CosmosClient(_watcherOption.GetResolvedConnectionString());
                Database database = cosmosClient.GetDatabase(_watcherOption.DatabaseName);

                Container traceContainer = await database.CreateContainerIfNotExistsAsync("Trace", Constants.DefaultPartitionKey);
                Container leaseContainer = await database.CreateContainerIfNotExistsAsync("_feedLease", Constants.DefaultPartitionKey);

                ChangeFeedProcessor changeFeedProcessor = traceContainer.GetChangeFeedProcessorBuilder<TraceRecord>("traceChangeFeed", HandleChangesAsync)
                    .WithInstanceName(nameof(WatcherCmd))
                    .WithLeaseContainer(leaseContainer)
                    .Build();

                await changeFeedProcessor.StartAsync();
                _logger.LogInformation("Feed process has started");

                try
                {
                    await Task.Delay(-1, tokenSource.Token);
                }
                catch { }
                finally
                {
                    await changeFeedProcessor.StopAsync();
                    _logger.LogInformation("Feed process has stopped");
                }

                cts.SetResult(true);
            });

            return cts.Task;
        }

        /// <summary>
        /// The delegate receives batches of changes as they are generated in the change feed and can process them.
        /// </summary>
        private Task HandleChangesAsync(IReadOnlyCollection<TraceRecord> changes, CancellationToken cancellationToken)
        {
            foreach (TraceRecord item in changes)
            {
                var items = item.GetPropertyValues();

                string line = new[]
                {
                    "Detected operation for item",
                }
                .Concat(items.Select((x, i) => $"{x.Key}={{v{i}}}"))
                .Select(x => x + Environment.NewLine)
                .Aggregate(string.Empty, (a, x) => a + x)
                .Func(x => x.Substring(0, x.Length));

                _logger.LogInformation(line, items.Select(x => x.Value).ToArray());
            }

            return Task.CompletedTask;
        }
    }
}
