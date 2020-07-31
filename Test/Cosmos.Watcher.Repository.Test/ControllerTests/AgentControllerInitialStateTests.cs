using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Xunit;

namespace Cosmos.Watcher.Repository.Test.ControllerTests
{
    public class AgentControllerInitialStateTests
    {
        private readonly ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private readonly ICosmosWatcherOption _watcherOption;
        private const string _databaseName = "testDatabase.agentController.initialState";
        private const string _fakeAgentText = "fakeAgent";

        public AgentControllerInitialStateTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [Fact]
        public async Task GivenNoAgentsAndNoTargets_WhenGetAssignments_NoTargetsReturn()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);
            await watcherRepository.InitializeEnvironment();

            IRecordContainer<AgentRecord> container = await watcherRepository.Container.Create<AgentRecord>();
            container.Should().NotBeNull();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());
            agentController.Should().NotBeNull();

            // Act
            await agentController.LoadBalanceAssignments();
            IReadOnlyList<TargetRecord> targetRecords = await agentController.GetAssignments(_fakeAgentText);

            // Assert
            targetRecords.Should().NotBeNull();
            targetRecords.Count.Should().Be(0);

            // Clean up
            await watcherRepository.Database.Delete(_databaseName);
        }

        [Fact]
        public async Task GivenOneAgentswithNoTargets_WhenGetAssignments_NoTargetsReturn()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);
            await watcherRepository.InitializeEnvironment();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());
            agentController.Should().NotBeNull();
            AgentRecord agentRecord = await agentController.CreateTestAgent(1);

            // Act
            await agentController.LoadBalanceAssignments();
            IReadOnlyList<TargetRecord> targetRecords = await agentController.GetAssignments(agentRecord.Id);

            // Assert
            targetRecords.Should().NotBeNull();
            targetRecords.Count.Should().Be(0);

            // Clean up
            await watcherRepository.Database.Delete(_databaseName);
        }

        [Fact]
        public async Task GiveNoAgentsAndOneTarget_WhenGetAssignments_NoTargetsReturn()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);
            await watcherRepository.InitializeEnvironment();

            IRecordContainer<TargetRecord> targetContainer = await watcherRepository.Container.Create<TargetRecord>();
            targetContainer.Should().NotBeNull();
            await targetContainer.CreateTestTarget(1);

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());
            agentController.Should().NotBeNull();

            // Act
            await agentController.LoadBalanceAssignments();
            IReadOnlyList<TargetRecord> targetRecords = await agentController.GetAssignments(_fakeAgentText);

            // Assert
            targetRecords.Should().NotBeNull();
            targetRecords.Count.Should().Be(0);

            // Clean up
            await watcherRepository.Database.Delete(_databaseName);
        }
    }
}
