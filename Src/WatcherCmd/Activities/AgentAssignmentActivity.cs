using System;
using System.Collections.Generic;
using System.Text;
using WatcherCmd.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Toolbox.Tools;
using System.Threading.Tasks;
using System.Threading;
using Toolbox.Services;
using Microsoft.Extensions.Logging;
using WatcherCmd.Tools;
using System.IO;

namespace WatcherCmd.Activities
{
    internal class AgentAssignmentActivity : ActivityEntityBase<AgentAssignmentRecord>
    {
        public AgentAssignmentActivity(IOption option, IRecordContainer<AgentAssignmentRecord> recordContainer, IJson json, ILogger<AgentAssignmentActivity> logger)
            : base(option, recordContainer, json, logger, "Agent Assignment")
        {
        }

        public override Task CreateTemplate(CancellationToken token)
        {
            var record = new AgentAssignmentRecord("{agentId}", "{targetId}");

            File.WriteAllText(_option.File, _json.Serialize(record));
            _logger.LogInformation($"Create json template {_option.File} for Agent Assignment");

            return Task.CompletedTask;
        }
    }
}
