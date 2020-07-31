using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Services;
using Watcher.Cosmos.Repository;
using WatcherCmd.Application;
using WatcherCmd.Test.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Xunit;

namespace WatcherCmd.Test.Activities
{
    public class AgentActivityTest
    {
        private readonly ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();

        [Fact]
        public async Task ExecuteAgentCreate_ShouldBeSuccessful()
        {
            var agentRecord = new AgentRecord
            {
                Id = "agent_1",
                State = AgentState.Running,
            };

            string file = TestTools.WriteResourceToTempFile(nameof(AgentRecord), agentRecord);

            try
            {
                var args = new[]
                {
                    "Agent",
                    "Create",
                    $"File={file}"
                };

                TestConfiguration testConfiguration = new TestConfiguration();
                string[] programArgs = testConfiguration.BuildArgs(args);
                IOption option = testConfiguration.GetOption(args);

                await Program.Main(programArgs);

                IWatcherRepository watcherRepository = new CosmosWatcherRepository(option.Store, _loggerFactory);
                IRecordContainer<AgentRecord> container = await watcherRepository.Container.Create<AgentRecord>(option.Store.DatabaseName);

                Record<AgentRecord>? read = await container.Get(agentRecord.Id);
                read.Should().NotBeNull();
                (agentRecord == read!.Value).Should().BeTrue();
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}
