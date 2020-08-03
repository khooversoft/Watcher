using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using WatcherAgent.Application;
using WatcherSdk.Host;
using WatcherSdk.Probe;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherAgent.Services
{
    internal class JobHost : IHostedService
    {
        private readonly IOption _option;
        private readonly IRecordContainer<TargetRecord> _targetContainer;
        private readonly IRecordContainer<TraceRecord> _traceRecord;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<JobHost> _logger;
        private IAgentHost? _agentHost;
        private TimerAsync? _timerAsync;

        public JobHost(
            IOption option,
            IRecordContainer<TargetRecord> targetContainer,
            IRecordContainer<TraceRecord> traceRecord,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            option.VerifyNotNull(nameof(option));
            targetContainer.VerifyNotNull(nameof(targetContainer));
            traceRecord.VerifyNotNull(nameof(traceRecord));
            httpClientFactory.VerifyNotNull(nameof(httpClientFactory));
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _option = option;
            _targetContainer = targetContainer;
            _traceRecord = traceRecord;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory.CreateLogger<JobHost>();
        }

        public async Task StartAsync(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(StartAsync)}: Starting Job Host");

            Interlocked.CompareExchange(
                ref _timerAsync,
                new TimerAsync(_option.StoreSyncFrequency, (id, token) => SyncAssignments(token), _loggerFactory.CreateLogger<TimerAsync>()),
                null);

            _timerAsync.Start(token);
            await SyncAssignments(token);
        }

        public async Task StopAsync(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(StopAsync)}: Starting Job Host");

            await StopAgentHost();
            Interlocked.Exchange(ref _timerAsync, null)?.Stop();
        }

        public async Task SyncAssignments(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(SyncAssignments)}: Syncing assignments");
            await StopAgentHost();

            _logger.LogInformation($"{nameof(SyncAssignments)}: Starting Job Host");
            IReadOnlyList<TargetRecord> currentAssigned = await _targetContainer.GetAssignments(_option.AgentId, _logger, token);

            _agentHost = new AgentHostBuilder()
                .SetAgentId(_option.AgentId)
                .SetTraceContainer(_traceRecord)
                .SetProbeFactory(new ProbeFactory(_httpClientFactory, _loggerFactory))
                .SetLoggerFactory(_loggerFactory)
                .AddTarget(currentAssigned.ToArray())
                .Build();
        }

        private async Task StopAgentHost()
        {
            IAgentHost? agentHost = Interlocked.Exchange(ref _agentHost, null);
            if (agentHost != null) await agentHost.Stop();
        }
    }
}
