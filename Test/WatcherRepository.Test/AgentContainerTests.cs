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
    public class AgentContainerTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private IWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase.agent";

        public AgentContainerTests()
        {
            _loggerFactory = new TestLoggerBuilder().Build();
            _watcherOption = new TestOptionBuilder().Build();
        }


        [TestMethod]
        public async Task GivenAgent_WhenRoundTrip_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None);

            WatcherContainer<AgentRecord> container = await watcherRepository.CreateContainer<AgentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new AgentRecord
            {
                Id = "Agent1",
                State = AgentState.Running,
                UtcHeartbeat = DateTime.UtcNow,
            };

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<AgentRecord> activeList = await container.Search($"select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Act - Read
            AgentRecord? read = await container.Get(record.Id);
            read.Should().NotBeNull();
            (read == record).Should().BeTrue();

            // Act - Delete
            (await container.Delete(record.Id, etag)).Should().BeTrue();

            IReadOnlyList<AgentRecord> deleteList = await container.Search($"select * from ROOT");
            deleteList.Should().NotBeNull();
            deleteList.Count.Should().Be(0);

            // Clean up
            (await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None)).Should().BeTrue();
        }


        [TestMethod]
        public async Task GivenAgent_WhenDuplicatedAdded_ShouldSuccessAsUpdate()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName, CancellationToken.None);

            WatcherContainer<AgentRecord> container = await watcherRepository.CreateContainer<AgentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new AgentRecord
            {
                Id = "Agent1",
                State = AgentState.Running,
                UtcHeartbeat = DateTime.UtcNow,
            };

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<AgentRecord> activeList = await container.Search($"select * from ROOT");
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
