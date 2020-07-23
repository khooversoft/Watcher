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
using WatcherRepository.Models;
using WatcherRepository.Records;
using WatcherRepository.Test.Application;

namespace WatcherRepository.Test.RecordTests
{
    [TestClass]
    public class AgentAssignmentTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private IWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase.assignment";

        public AgentAssignmentTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [TestMethod]
        public async Task GivenAgentAssignment_WhenRoundTrip_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<AgentAssignmentRecord> container = await watcherRepository.Container.Create<AgentAssignmentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new Record<AgentAssignmentRecord>(new AgentAssignmentRecord("agent", "target"));

            ETag eTag = await container.Set(record);
            eTag.Should().NotBeNull();

            IReadOnlyList<AgentAssignmentRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Act - Read
            Record<AgentAssignmentRecord>? read = await container.Get(record.Value.Id);
            read.Should().NotBeNull();
            read!.Value.Should().NotBeNull();
            (read.Value == record.Value).Should().BeTrue();

            // Act - Delete
            (await container.Delete(record.Value.Id, read.ETag)).Should().BeTrue();

            IReadOnlyList<AgentAssignmentRecord> deleteList = await container.Search("select * from ROOT");
            deleteList.Should().NotBeNull();
            deleteList.Count.Should().Be(0);

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenAgentAssignment_WhenDuplicatedAdded_ShouldSuccessAsUpdate()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<AgentAssignmentRecord> container = await watcherRepository.Container.Create<AgentAssignmentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var item = new AgentAssignmentRecord("agent", "target");

            ETag eTag = await container.Set(item);
            eTag.Should().NotBeNull();

            IReadOnlyList<AgentAssignmentRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            ETag dup = await container.Set(item);
            dup.Should().NotBeNull();

            activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }


        [TestMethod]
        public async Task GivenMultipleAgentAssignments_WhenRoundTrip_ShouldSuccess()
        {
            const int max = 10;

            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<AgentAssignmentRecord> container = await watcherRepository.Container.Create<AgentAssignmentRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            IReadOnlyList<AgentAssignmentRecord> records = Enumerable.Range(0, max)
                .Select((x, i) => new AgentAssignmentRecord($"agent.{i}", "target"))
                .ToArray();

            var eTags = new List<ETag>();
            foreach (AgentAssignmentRecord item in records)
            {
                ETag eTag = await container.Set(item);
                eTag.Should().NotBeNull();
                eTag.Value.Should().NotBeEmpty();

                eTags.Add(eTag);
            }

            // Act - list records
            IReadOnlyList<AgentAssignmentRecord> activeList = await container.Search("select * from ROOT");
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
                IReadOnlyList<AgentAssignmentRecord> queryList = await container.Search($"select * from ROOT r where r.id = \"{record.Id}\"");
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
