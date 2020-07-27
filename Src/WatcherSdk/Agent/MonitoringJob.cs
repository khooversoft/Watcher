using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using WatcherSdk.Probe;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherSdk.Agent
{
    public class MonitoringJob : IMonitoringJob
    {
        private readonly TargetRecord _targetRecord;
        private readonly IProbe _probe;
        private readonly ILogger<MonitoringJob> _logger;
        private readonly Random _random = new Random();
        private CancellationTokenSource? _cancellation;

        public MonitoringJob(TargetRecord targetRecord, IProbe probe, ILogger<MonitoringJob> logger)
        {
            targetRecord.VerifyNotNull(nameof(targetRecord))
                .VerifyAssert(x => !x.Url.IsEmpty(), x => nameof(x.Url))
                .VerifyAssert(x => x.Frequency != null, x => nameof(x.Frequency));

            probe.VerifyNotNull(nameof(probe));
            logger.VerifyNotNull(nameof(logger));

            _targetRecord = targetRecord;
            _probe = probe;
            _logger = logger;
        }

        public Task Start(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();

            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            _logger.LogInformation($"{nameof(Start)}: Target={_targetRecord}");

            _ = Task.Run(async () =>
            {
                TimeSpan delay = TimeSpan.FromSeconds(_random.Next(10, 30));
                await Task.Delay(delay);

                while (!token.IsCancellationRequested)
                {
                    _logger.LogInformation($"{nameof(Start)}: Pinging target={_targetRecord}");
                    await _probe.Ping(token);

                    await Task.Delay((TimeSpan)_targetRecord.Frequency!);
                }

                tcs.SetResult(true);
            });

            return tcs.Task;
        }
    }
}
