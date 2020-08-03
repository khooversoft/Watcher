using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace WatcherSdk.Host
{
    public class AgentHost : IAgentHost
    {
        private readonly IEnumerable<IMonitoringJob> _monitoringJobs;
        private readonly ILogger<AgentHost> _logger;
        private CancellationTokenSource? _cancellation;
        private Task? _runningTasks;

        public AgentHost(IEnumerable<IMonitoringJob> monitorJobs, ILogger<AgentHost> logger)
        {
            monitorJobs
                .VerifyNotNull(nameof(monitorJobs))
                .VerifyAssert(x => x.Count() > 0, $"No jobs");

            logger.VerifyNotNull(nameof(logger));
            _monitoringJobs = monitorJobs;
            _logger = logger;
        }

        public Task Start(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Start)}: Starting monitoring jobs");

            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            var tasks = new List<Task>();

            foreach (var job in _monitoringJobs)
            {
                tasks.Add(job.Run(_cancellation.Token));
            }

            _runningTasks = Task.WhenAll(tasks);
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _cancellation.VerifyNotNull("Host is not running");
            _runningTasks.VerifyNotNull("No running tasks");

            _cancellation.Cancel();
            return _runningTasks;
        }
    }
}
