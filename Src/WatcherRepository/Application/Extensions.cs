using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;
using WatcherRepository.Records;

namespace WatcherRepository.Application
{
    public static class Extensions
    {
        public static bool IsAgentRunning(this AgentRecord agentRecord, TimeSpan offlineTolerance)
        {
            agentRecord.VerifyNotNull(nameof(agentRecord));

            return agentRecord.State == Models.AgentState.Running &&
                (DateTime.UtcNow - agentRecord.UtcHeartbeat) <= offlineTolerance;
        }
    }
}
