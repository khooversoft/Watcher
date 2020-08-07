using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using WatcherSdk.Records;

namespace WatcherSdk.Repository
{
    public static class Extensions
    {
        public static Task<IReadOnlyList<TargetRecord>> GetAssignments(this IRecordContainer<TargetRecord> container, string agentId, ILogger logger, CancellationToken token = default)
        {
            agentId = agentId
                .VerifyNotEmpty(nameof(agentId))
                .ToLowerInvariant();

            var parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("agentId", agentId),
            };

            string search = $"select * from ROOT r where r.AssignedAgentId = @agentId";

            logger.LogInformation($"{nameof(GetAssignments)}: Searching for agent assignments, search={search.WithParameters(parameters)}");
            return container.Search(search, parameters, token);
        }
    }
}
