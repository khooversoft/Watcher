using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Xunit;

namespace Cosmos.Watcher.Repository.Test.ControllerTests
{
    public class AgentControllerAssignmentTests
    {
        private readonly ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private readonly ICosmosWatcherOption _watcherOption;
        private const string _databaseName = "testDatabase.agentController.assignments";

        public AgentControllerAssignmentTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [Fact]
        public async Task GiveOneAgentsAddingTargets_WhenGetAssignments_CorrectTargetsShouldBeReturned()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
            await watcherRepository.InitializeContainers();

            IRecordContainer<TargetRecord> targetContainer = await watcherRepository.Container.Create<TargetRecord>();
            targetContainer.Should().NotBeNull();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());

            // Act
            const int max = 10;
            AgentRecord agentRecord = await agentController.CreateTestAgent(1);

            foreach (int targetCount in Enumerable.Range(1, max))
            {
                await targetContainer.CreateTestTarget(targetCount);

                await agentController.LoadBalanceAssignments();
                IReadOnlyList<TargetRecord> assignments = await targetContainer.GetAssignments(agentRecord.Id, _loggerFactory.CreateLogger<TargetRecord>());
                assignments.Count.Should().BeGreaterThan(targetCount);
            }

            // Clean up
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
        }

        [Fact]
        public async Task GiveAgentsAddingTargets_WhenGetAssignments_CorrectTargetsShouldBeReturned()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
            await watcherRepository.InitializeContainers();

            IRecordContainer<TargetRecord> targetContainer = await watcherRepository.Container.Create<TargetRecord>();
            targetContainer.Should().NotBeNull();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());

            // Act
            const int max = 10;
            var agents = new List<AgentRecord>();

            foreach (int targetCount in Enumerable.Range(1, max))
            {
                await targetContainer.CreateTestTarget(targetCount);
            }

            foreach (int agentCount in Enumerable.Range(1, max))
            {
                agents.Add(await agentController.CreateTestAgent(agentCount));

                foreach (var agent in agents)
                {
                    await agentController.LoadBalanceAssignments();
                    IReadOnlyList<TargetRecord> assignments = await targetContainer.GetAssignments(agent.Id, _loggerFactory.CreateLogger<TargetRecord>());
                    assignments.Count.Should().BeGreaterThan(0);
                }
            }

            // Clean up
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
        }
    }
}
