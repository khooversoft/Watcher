using System;
using System.Collections.Generic;
using System.Text;
using WatcherCmd.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Toolbox.Tools;
using System.Threading.Tasks;
using System.Threading;

namespace WatcherCmd.Activities
{
    internal class BalanceActivity
    {
        private readonly IOption _option;
        private readonly IAgentController _agentController;

        public BalanceActivity(IOption option, IAgentController agentController)
        {
            option.VerifyNotNull(nameof(option));
            agentController.VerifyNotNull(nameof(agentController));

            _option = option;
            _agentController = agentController;
        }

        public Task BalanceAgents(CancellationToken token) => _agentController.LoadBalanceAssignments(token);
    }
}
