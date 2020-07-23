using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatcherRepository.Application;
using WatcherRepository.Records;
using WatcherRepository.Test.Application;

namespace WatcherRepository.Test.ControllerTests
{
    [TestClass]
    public class AgentControllerAssignmentTests
    {
        private readonly ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private readonly IWatcherOption _watcherOption;
        private const string _databaseName = "testDatabase.agentController.assignments";

        public AgentControllerAssignmentTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [TestMethod]
        public async Task GiveOneAgentsAddingTargets_WhenGetAssignments_CorrectTargetsShouldBeReturned()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
            await watcherRepository.InitializeEnvironment();

            IRecordContainer<TargetRecord> targetContainer = await watcherRepository.Container.Create<TargetRecord>();
            targetContainer.Should().NotBeNull();

            IRecordContainer<AgentAssignmentRecord> assignmentContainer = await watcherRepository.Container.Create<AgentAssignmentRecord>();

            IAgentController agentController = new AgentController(_watcherOption, watcherRepository, _loggerFactory.CreateLogger<AgentController>());

            // Act
            const int max = 10;
            AgentRecord agentRecord = await agentController.CreateTestAgent(1);

            foreach (int targetCount in Enumerable.Range(1, max))
            {
                await targetContainer.CreateTestTarget(targetCount);

                await agentController.LoadBalanceAssignments();
                IReadOnlyList<TargetRecord> assignments = await agentController.GetAssignments(agentRecord.Id);
                assignments.Count.Should().BeGreaterThan(targetCount);

                IReadOnlyList<AgentAssignmentRecord> agentAssignmentRecords = await assignmentContainer.ListAll();
                agentAssignmentRecords.Count.Should().BeGreaterThan(targetCount);
            }

            // Clean up
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
        }

        [TestMethod]
        public async Task GiveAgentsAddingTargets_WhenGetAssignments_CorrectTargetsShouldBeReturned()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
            await watcherRepository.InitializeEnvironment();

            IRecordContainer<TargetRecord> targetContainer = await watcherRepository.Container.Create<TargetRecord>();
            targetContainer.Should().NotBeNull();

            IRecordContainer<AgentAssignmentRecord> assignmentContainer = await watcherRepository.Container.Create<AgentAssignmentRecord>();

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
                    IReadOnlyList<TargetRecord> assignments = await agentController.GetAssignments(agent.Id);
                    assignments.Count.Should().BeGreaterThan(0);

                    IReadOnlyList<AgentAssignmentRecord> agentAssignmentRecords = await assignmentContainer.ListAll();
                    agentAssignmentRecords.Count.Should().BeGreaterThan(0);
                }
            }

            // Clean up
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);
        }
    }
}
