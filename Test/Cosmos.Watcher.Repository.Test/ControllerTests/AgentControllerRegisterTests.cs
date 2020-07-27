using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace Cosmos.Watcher.Repository.Test.ControllerTests
{
    [TestClass]
    public class AgentControllerRegisterTests
    {
        private readonly ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private readonly ICosmosWatcherOption _watcherOption;
        private const string _databaseName = "testDatabase.agentController.register";

        public AgentControllerRegisterTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [TestMethod]
        public async Task GivenAgent_WhenRegistgered_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);
            await watcherRepository.InitializeEnvironment();

            IRecordContainer<AgentRecord> agentContainer = await watcherRepository.Container.Create<AgentRecord>();
            agentContainer.Should().NotBeNull();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());

            // Act - verify agent is not register
            const string agentName = "Agent.1";
            (await agentContainer.Exist(agentName)).Should().BeFalse();
            (await agentContainer.Get(agentName)).Should().BeNull();

            // Act - register agent and verify
            var agentRecord = new AgentRecord
            {
                Id = agentName,
                State = AgentState.Running,
            };

            await agentController.Register(agentRecord);
            (await agentContainer.Exist(agentName)).Should().BeTrue();

            Record<AgentRecord>? record = await agentContainer.Get(agentName, token: CancellationToken.None);
            record.Should().NotBeNull();
            (record!.Value == agentRecord).Should().BeTrue();

            // Act - Un-register agent
            await agentController.UnRegister(agentName);
            (await agentContainer.Exist(agentName)).Should().BeFalse();
            (await agentContainer.Get(agentName)).Should().BeNull();

            // Clean up
            await watcherRepository.Database.Delete(_databaseName);
        }

        [TestMethod]
        public async Task GivenAgent_WhenRegistgeredAndStateIsChange_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);
            await watcherRepository.InitializeEnvironment();

            IRecordContainer<AgentRecord> container = await watcherRepository.Container.Create<AgentRecord>();
            container.Should().NotBeNull();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());

            // Act - verify agent is not register
            const string agentName = "Agent.1";

            // Act - register agent and verify
            var agentRecord = new AgentRecord
            {
                Id = agentName,
                State = AgentState.Running,
            };

            await agentController.Register(agentRecord);
            (await container.Exist(agentName)).Should().BeTrue();

            Record<AgentRecord>? record = await container.Get(agentName, token: CancellationToken.None);
            record.Should().NotBeNull();
            (record!.Value == agentRecord).Should().BeTrue();

            await agentController.SetState(agentName, AgentState.Stopped);

            agentRecord.State = AgentState.Stopped;

            record = await container.Get(agentName, token: CancellationToken.None);
            record.Should().NotBeNull();
            (record!.Value == agentRecord).Should().BeTrue();

            // Act - Un-register agent
            await agentController.UnRegister(agentName);
            (await container.Exist(agentName)).Should().BeFalse();

            // Clean up
            await watcherRepository.Database.Delete(_databaseName);
        }
    }
}
