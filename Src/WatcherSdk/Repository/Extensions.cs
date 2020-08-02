using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using WatcherSdk.Records;

namespace WatcherSdk.Repository
{
    public static class Extensions
    {
        public static Task<IReadOnlyList<AgentAssignmentRecord>> GetAssignmentsForAgent(this IRecordContainer<AgentAssignmentRecord> container, string agentId, ILogger logger, CancellationToken token)
        {
            agentId.VerifyNotEmpty(nameof(agentId));

            string search = $"select * from ROOT where AgentId = \"{agentId}\"";

            logger.LogInformation($"{nameof(GetAssignmentsForAgent)}: Searching for agent assignments, search={search}");
            return container.Search(search, token);
        }
    }
}
