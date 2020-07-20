using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatcherRepository.Models;
using WatcherRepository.Records;
using WatcherRepository.Test.Application;

namespace WatcherRepository.Test
{
    [TestClass]
    public class AgentAssignmentTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private IWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase.assignment";

        public AgentAssignmentTests()
        {
            _loggerFactory = new TestLoggerBuilder().Build();
            _watcherOption = new TestOptionBuilder().Build();
        }


        [TestMethod]
        public async Task GivenAgentAssignment_WhenRoundTrip_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None);

            WatcherContainer<AgentAssignmentRecord> container = await watcherRepository.CreateContainer<AgentAssignmentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new AgentAssignmentRecord
            {
                Id = "agent.target",
                AgentId = "agent",
                TargetId = "target",
            };

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<AgentAssignmentRecord> activeList = await container.Search($"select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Act - Read
            AgentAssignmentRecord? read = await container.Get(record.Id);
            read.Should().NotBeNull();
            (read == record).Should().BeTrue();

            // Act - Delete
            (await container.Delete(record.Id, etag)).Should().BeTrue();

            IReadOnlyList<AgentAssignmentRecord> deleteList = await container.Search($"select * from ROOT");
            deleteList.Should().NotBeNull();
            deleteList.Count.Should().Be(0);

            // Clean up
            (await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenAgentAssignment_WhenDuplicatedAdded_ShouldSuccessAsUpdate()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None);

            WatcherContainer<AgentAssignmentRecord> container = await watcherRepository.CreateContainer<AgentAssignmentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new AgentAssignmentRecord
            {
                Id = "agent.target",
                AgentId = "agent",
                TargetId = "target",
            };

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<AgentAssignmentRecord> activeList = await container.Search($"select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            ETag dup = await container.Set(record);
            dup.Should().NotBeNull();

            activeList = await container.Search($"select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Clean up
            (await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None)).Should().BeTrue();
        }
    }
}
