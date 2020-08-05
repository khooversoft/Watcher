using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Services;
using WatcherCmd.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;

namespace WatcherCmd.Activities
{
    internal class TemplateActivity
    {
        private readonly IOption _option;
        private readonly IJson _json;
        private readonly ILogger<TemplateActivity> _logger;

        public TemplateActivity(IOption option, IJson json, ILogger<TemplateActivity> logger)
        {
            _option = option;
            _json = json;
            _logger = logger;
        }

        public Task Create()
        {
            switch (_option)
            {
                case Option option when option.Agent:
                    CreateAgentTemplate(0, _option.File!);
                    break;

                case Option option when option.Target:
                    CreateTargetTemplate(0, _option.File!);
                    break;
            }

            return Task.CompletedTask;
        }

        private void CreateAgentTemplate(int index, string file)
        {
            var record = new AgentRecord
            {
                Id = $"Agent_{index}",
                State = AgentState.Running,
                UtcHeartbeat = DateTime.UtcNow,
            };

            File.WriteAllText(file, _json.SerializeFormat(record));
            _logger.LogInformation($"Create json template {file} for Agent");
        }

        private void CreateTargetTemplate(int index, string file)
        {
            var record = new TargetRecord
            {
                Id = $"Target_{index}",
                Description = $"Target_{index} description",
                ReadyUrl = $"http://localhost:{index + 5000}/ping/ready",
                RunningUrl = $"http://localhost:{index + 5000}/ping/running",
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
                TargetType = "REST",
                Enabled = true,
                FrequencyInSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds,
            };

            File.WriteAllText(file, _json.Serialize(record));
            _logger.LogInformation($"Create json template {file} for Agent Assignment");
        }
    }
}