using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace Cosmos.Watcher.Repository.Test.RecordTests
{
    [TestClass]
    public class AgentTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private ICosmosWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase.agent";

        public AgentTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [TestMethod]
        public async Task GivenAgent_WhenRoundTrip_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<AgentRecord> container = await watcherRepository.Container.Create<AgentRecord>(_databaseName);
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

            IReadOnlyList<AgentRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Act - Read
            Record<AgentRecord>? read = await container.Get(record.Id);
            read.Should().NotBeNull();
            (read!.Value == record).Should().BeTrue();

            // Act - Delete
            (await container.Delete(record.Id, etag)).Should().BeTrue();

            IReadOnlyList<AgentRecord> deleteList = await container.Search("select * from ROOT");
            deleteList.Should().NotBeNull();
            deleteList.Count.Should().Be(0);

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }


        [TestMethod]
        public async Task GivenAgent_WhenDuplicatedAdded_ShouldSuccessAsUpdate()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<AgentRecord> container = await watcherRepository.Container.Create<AgentRecord>(_databaseName);
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

            IReadOnlyList<AgentRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            ETag dup = await container.Set(record);
            dup.Should().NotBeNull();

            activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenMultipleAgents_WhenRoundTrip_ShouldSuccess()
        {
            const int max = 10;

            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<AgentRecord> container = await watcherRepository.Container.Create<AgentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            IReadOnlyList<AgentRecord> records = Enumerable.Range(0, max)
                .Select((x, i) => new AgentRecord
                {
                    Id = $"Agent1_{i}",
                    State = AgentState.Running,
                    UtcHeartbeat = DateTime.UtcNow,
                })
                .ToArray();

            var eTags = new List<ETag>();
            foreach (AgentRecord item in records)
            {
                ETag eTag = await container.Set(item);
                eTag.Should().NotBeNull();
                eTag.Value.Should().NotBeEmpty();

                eTags.Add(eTag);
            }

            // Act - list records
            IReadOnlyList<AgentRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(max);

            records
                .Zip(activeList, (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();

            var deleteList = records
                .Zip(eTags, (o, i) => (record: o, eTag: i))
                .ToArray();

            // Act - lookup each record
            foreach (var record in activeList)
            {
                IReadOnlyList<AgentRecord> queryList = await container.Search($"select * from ROOT r where r.id = \"{record.Id}\"");
                queryList.Should().NotBeNull();
                queryList.Count.Should().Be(1);
                (record == queryList.Single()).Should().BeTrue();
            }

            // Act - delete all records
            foreach (var record in deleteList)
            {
                (await container.Delete(record.record.Id, record.eTag)).Should().BeTrue();
            }

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }
    }
}
