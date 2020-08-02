using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WatcherAgent.Application;
using WatcherAgent.Services.AgentAssignment;

namespace WatcherAgent.Services.Work
{
    internal class JobHost : IHostedService
    {
        private readonly IOption _option;
        private readonly IAgentAssignmentService _agentAssignmentService;
        private readonly LoggerFactory _loggerFactory;
        private readonly ILogger<JobHost> _logging;

        public JobHost(IOption option, IAgentAssignmentService agentAssignmentService, LoggerFactory loggerFactory)
        {
            _option = option;
            _agentAssignmentService = agentAssignmentService;
            _loggerFactory = loggerFactory;

            _logging = _loggerFactory.CreateLogger<JobHost>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
