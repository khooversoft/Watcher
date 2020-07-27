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
    internal class AgentActivity
    {
        private readonly IOption _option;
        private readonly IRecordContainer<AgentRecord> _agentRecordContainer;

        public AgentActivity(IOption option, IRecordContainer<AgentRecord> agentRecordContainer)
        {
            option.VerifyNotNull(nameof(option));
            agentRecordContainer.VerifyNotNull(nameof(agentRecordContainer));

            _option = option;
            _agentRecordContainer = agentRecordContainer;
        }

        public Task Create(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task List(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task Delete(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task Clear(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
