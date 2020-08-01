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
using WatcherCmd.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WatcherSdk.Models;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace WatcherCmd.Activities
{
    internal class AgentActivity : ActivityEntityBase<AgentRecord>
    {
        public AgentActivity(IOption option, IRecordContainer<AgentRecord> recordContainer, IJson json, ILogger<AgentActivity> logger)
            : base(option, recordContainer, json, logger, "Agent")
        {
        }

        public override Task CreateTemplate(CancellationToken token)
        {
            var record = new AgentRecord
            {
                Id = "{agentId}",
                State = AgentState.Running,
                UtcHeartbeat = DateTime.UtcNow,
            };

            File.WriteAllText(_option.File, _json.Serialize(record));
            _logger.LogInformation($"Create json template {_option.File} for Agent");

            return Task.CompletedTask;
        }
    }
}
