using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace Cosmos.Watcher.Repository.Test.Application
{
    public static class Extensions
    {
        public static async Task<AgentRecord> CreateTestAgent(this IAgentController container, int index)
        {
            var agentRecord = new AgentRecord
            {
                Id = $"Agent.{index}",
                State = AgentState.Running,
            };

            await container.Register(agentRecord, CancellationToken.None);

            return agentRecord;
        }

        public static async Task<TargetRecord> CreateTestTarget(this IRecordContainer<TargetRecord> container, int index)
        {
            var target = new TargetRecord
            {
                Id = $"Target.{index}",
                Description = "test target",
                Enabled = true,
            };

            await container.Set(target);

            return target;
        }
    }
}
