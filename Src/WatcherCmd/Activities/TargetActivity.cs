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
using WatcherSdk.Models;
using System.Net;

namespace WatcherCmd.Activities
{
    internal class TargetActivity : ActivityEntityBase<TargetRecord>
    {
        public TargetActivity(IOption option, IRecordContainer<TargetRecord> recordContainer, IJson json, ILogger<TargetActivity> logger)
            : base(option, recordContainer, json, logger, "Target")
        {
        }

        public override Task CreateTemplate(CancellationToken token)
        {
            var record = new TargetRecord
            {
                Id = "{targetId}",
                Description = "{target description}",
                ReadyUrl = "{url of resource}",
                StatusCodeMaps = new StatusCodeMap[]
                {
                    new StatusCodeMap { HttpStatusCode = HttpStatusCode.OK, State = TargetState.Ok },
                    new StatusCodeMap { HttpStatusCode = HttpStatusCode.NotFound, State = TargetState.Error },
                },
                BodyElementMaps = new BodyElementMap[]
                {
                    new BodyElementMap { State = TargetState.Ok, Path = "/state", CompareTo = "success" },
                    new BodyElementMap { State = TargetState.Error, Path = "/state", CompareTo = "error" },
                },
                TargetType = "{target type, REST}",
                Enabled = true,
                FrequencyInSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds,
            };

            File.WriteAllText(_option.File, _json.Serialize(record));
            _logger.LogInformation($"Create json template {_option.File} for Agent Assignment");

            return Task.CompletedTask;
        }
    }
}
