using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using WatcherSdk.Probe;
using WatcherSdk.Records;

namespace WatcherSdk.Host
{
    public class MonitoringJob : IMonitoringJob
    {
        private readonly TargetRecord _targetRecord;
        private readonly IProbe _probe;
        private readonly ILogger<MonitoringJob> _logger;

        public MonitoringJob(TargetRecord targetRecord, IProbe probe, ILogger<MonitoringJob> logger)
        {
            targetRecord.VerifyNotNull(nameof(targetRecord))
                .VerifyAssert(x => !x.ReadyUrl.IsEmpty(), x => nameof(x.ReadyUrl))
                .VerifyAssert(x => x.FrequencyInSeconds != null, x => nameof(x.FrequencyInSeconds));

            probe.VerifyNotNull(nameof(probe));
            logger.VerifyNotNull(nameof(logger));

            _targetRecord = targetRecord;
            _probe = probe;
            _logger = logger;
        }

        public Task Run(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();

            _logger.LogInformation($"{nameof(Run)}: Target={_targetRecord}");

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        _logger.LogInformation($"{nameof(Run)}: Pinging target={_targetRecord}");
                        await _probe.Ping(token);

                        await Task.Delay(TimeSpan.FromSeconds((double)_targetRecord.FrequencyInSeconds!), token);
                    }

                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Monitoring (ping) failed, Target={_targetRecord}");
                    tcs.SetException(ex);
                    return;
                }
            });

            return tcs.Task;
        }
    }
}
