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
        //private readonly IOption _option;
        //private readonly IRecordContainer<AgentRecord> _recordContainer;
        //private readonly IJson _json;
        //private readonly ILogger<AgentActivity> _logger;

        public AgentActivity(IOption option, IRecordContainer<AgentRecord> recordContainer, IJson json, ILogger<AgentActivity> logger)
            : base(option, recordContainer, json, logger, "Agent")
        {
        }
        //public AgentActivity(IOption option, IRecordContainer<AgentRecord> recordContainer, IJson json, ILogger<AgentActivity> logger)
        //{
        //    option.VerifyNotNull(nameof(option));
        //    recordContainer.VerifyNotNull(nameof(recordContainer));
        //    json.VerifyNotNull(nameof(json));
        //    logger.VerifyNotNull(nameof(logger));

        //    _option = option;
        //    _recordContainer = recordContainer;
        //    _json = json;
        //    _logger = logger;
        //}

        //public async Task Create(CancellationToken token)
        //{
        //    _logger.LogInformation($"{nameof(Create)}: Reading file {_option.File} and writing to store");
        //    AgentRecord record = _option.File!.ReadAndDeserialize<AgentRecord>(_json);

        //    await _recordContainer.Set(record, token);
        //}

        //public async Task List(CancellationToken token)
        //{
        //    IReadOnlyList<AgentRecord> list = await _recordContainer.ListAll(token);

        //    _logger.LogInformation("Listing all agent records");
        //    foreach(var item in list)
        //    {
        //        _logger.LogInformation($"Record={item}");
        //    }
        //}

        //public async Task Delete(CancellationToken token)
        //{
        //    _logger.LogInformation($"{nameof(Create)}: Deleting ID={_option.Id}");
        //    await _recordContainer.Delete(_option.Id!, token: token);
        //}

        //public async Task Clear(CancellationToken token)
        //{
        //    _logger.LogInformation($"{nameof(Create)}: Deleting ID={_option.Id}");
        //    IReadOnlyList<AgentRecord> list = await _recordContainer.ListAll(token);

        //    foreach (var item in list)
        //    {
        //        await _recordContainer.Delete(item.Id);
        //    }
        //}

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
