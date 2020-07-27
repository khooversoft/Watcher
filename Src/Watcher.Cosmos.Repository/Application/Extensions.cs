using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;
using WatcherSdk.Models;
using WatcherSdk.Records;

namespace Watcher.Cosmos.Repository.Application
{
    public static class Extensions
    {
        public static bool IsAgentRunning(this AgentRecord agentRecord, TimeSpan offlineTolerance)
        {
            agentRecord.VerifyNotNull(nameof(agentRecord));

            return agentRecord.State == AgentState.Running &&
                DateTime.UtcNow - agentRecord.UtcHeartbeat <= offlineTolerance;
        }
    }
}
