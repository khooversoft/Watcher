using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Services;
using Toolbox.Tools;
using Toolbox.Extensions;
using WatcherCmd.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using System.Runtime.CompilerServices;

namespace WatcherCmd.Activities
{
    internal class ImportActivity
    {
        private readonly IOption _option;
        private readonly IRecordContainer<AgentRecord> _agentContainer;
        private readonly IRecordContainer<TargetRecord> _targetContainer;
        private readonly IJson _json;
        private readonly ILogger<ImportActivity> _logger;

        public ImportActivity(IOption option, IRecordContainer<AgentRecord> agentContainer, IRecordContainer<TargetRecord> targetContainer, IJson json, ILogger<ImportActivity> logger)
        {
            _option = option;
            _agentContainer = agentContainer;
            _targetContainer = targetContainer;
            _json = json;
            _logger = logger;
        }

        public async Task Import(CancellationToken token)
        {
            IReadOnlyList<string> files = GetFiles(_option.File!);

            foreach (var file in files)
            {
                _logger.LogInformation($"Importing configuration {file}");

                string json = File.ReadAllText(file);
                switch (_json.Deserialize<RecordBase>(json))
                {
                    case RecordBase v:
                        await WriteRecord(v.RecordType, json, token);
                        break;

                    default:
                        throw new ArgumentException($"Bad format");
                }
            }
        }

        private async Task WriteRecord(string recordType, string json, CancellationToken token)
        {
            switch (recordType)
            {
                case nameof(AgentRecord):
                    AgentRecord agentRecord = _json.Deserialize<AgentRecord>(json);
                    await _agentContainer.Set(agentRecord, token);
                    break;

                case nameof(TargetRecord):
                    TargetRecord targetRecord = _json.Deserialize<TargetRecord>(json);
                    await _targetContainer.Set(targetRecord, token);
                    break;

                default:
                    throw new ArgumentException($"Unknown record type for importing, recordType={recordType}");
            }
        }

        private IReadOnlyList<string> GetFiles(string file)
        {
            if (Directory.Exists(file))
            {
                return Directory.GetFiles(file, "*.json", SearchOption.TopDirectoryOnly);
            }

            bool recursiveFolderSearch = file.EndsWith("\\**");
            bool folderSearch = file.EndsWith("\\*");
            string folder = Path.GetDirectoryName(file)!;
            string search = recursiveFolderSearch || folderSearch ? "*.json" : Path.GetFileName(file);

            _logger.LogInformation($"{nameof(GetFiles)}: Searching folder={folder}, search={search}");
            string[] files = Directory.GetFiles(folder, search, recursiveFolderSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            files.VerifyAssert(x => x.Length > 0, $"File(s) {file} does not exist");
            return files;
        }
    }
}
