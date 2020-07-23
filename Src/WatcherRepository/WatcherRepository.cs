using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using WatcherRepository.Application;
using WatcherRepository.Records;

namespace WatcherRepository
{
    public class WatcherRepository : IWatcherRepository
    {
        private readonly ILogger<WatcherRepository> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IWatcherOption _watcherOption;

        public WatcherRepository(IWatcherOption watcherOption, ILoggerFactory loggerFactory)
        {
            watcherOption.VerifyNotNull(nameof(watcherOption)).Verify();
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _watcherOption = watcherOption;
            _logger = loggerFactory.CreateLogger<WatcherRepository>();

            _cosmosClient = new CosmosClient(watcherOption.GetResolvedConnectionString());

            Database = new RepositoryDatabase(_cosmosClient, loggerFactory.CreateLogger<RepositoryDatabase>());
            Container = new RepositoryContainer(watcherOption, _cosmosClient, loggerFactory, loggerFactory.CreateLogger<RepositoryContainer>());
        }

        public IRepositoryDatabase Database { get; }

        public IRepositoryContainer Container { get; }

        public async Task InitializeEnvironment(CancellationToken token = default)
        {
            _logger.LogInformation($"{nameof(InitializeEnvironment)} - Initializing database with required containers");

            await Database.Create(_watcherOption.DatabaseName, token);

            await Container.Create<AgentRecord>(token: token);
            await Container.Create<AgentAssignmentRecord>(token: token);
            await Container.Create<TargetRecord>(token: token);
            await Container.Create<TraceRecord>(_watcherOption.TraceTraceTTL, token: token);
        }
    }
}
