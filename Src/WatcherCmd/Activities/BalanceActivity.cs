using System;
using System.Collections.Generic;
using System.Text;
using WatcherCmd.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Toolbox.Tools;
using System.Threading.Tasks;
using System.Threading;
using Watcher.Cosmos.Repository;

namespace WatcherCmd.Activities
{
    internal class BalanceActivity
    {
        private readonly IAgentController _agentController;

        public BalanceActivity(IAgentController agentController)
        {
            agentController.VerifyNotNull(nameof(agentController));

            _agentController = agentController;
        }

        public Task BalanceAgents(CancellationToken token) => _agentController.LoadBalanceAssignments(token);
    }
}
