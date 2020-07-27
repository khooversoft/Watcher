using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using WatcherSdk.Probe;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherSdk.Agent
{
    public class AgentHost : IAgentHost
    {
        private readonly IEnumerable<IMonitoringJob> _probs;
        private readonly ILogger<AgentHost> _logger;
        private CancellationTokenSource? _cancellation;

        public AgentHost(IEnumerable<IMonitoringJob> monitorJobs, ILogger<AgentHost> logger)
        {
            monitorJobs
                .VerifyNotNull(nameof(monitorJobs))
                .VerifyAssert(x => x.Count() > 0, $"No jobs");

            logger.VerifyNotNull(nameof(logger));
            _probs = monitorJobs;
            _logger = logger;
        }

        public async Task Start(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();

            _cancellation = new CancellationTokenSource();
            _logger.LogInformation($"{nameof(Start)}: Starting monitoring jobs");

            var tasks = new List<Task>();

            foreach (var probe in _probs)
            {
                tasks.Add(probe.Start(token));
            }

            await Task.WhenAll(tasks);
        }
    }
}
