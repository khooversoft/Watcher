using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace Cosmos.Watcher.Repository.Test.RecordTests
{
    [TestClass]
    public class TraceTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private ICosmosWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase.trace";

        public TraceTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [TestMethod]
        public async Task GivenAgentAssignment_WhenRoundTrip_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<TraceRecord> container = await watcherRepository.Container.Create<TraceRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new TraceRecord
            {
                Id = Guid.NewGuid().ToString(),
                AgentId = "agent",
                TargetId = "target",
                Url = "http://localhost",
                HttpStatusCode = HttpStatusCode.BadRequest,
                Body = "this is the body",
                TargetState = TargetState.Ok
            };

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<TraceRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Act - Read
            Record<TraceRecord>? read = await container.Get(record.Id);
            read.Should().NotBeNull();
            (read!.Value == record).Should().BeTrue();

            // Act - Delete
            (await container.Delete(record.Id, etag)).Should().BeTrue();

            IReadOnlyList<TraceRecord> deleteList = await container.Search("select * from ROOT");
            deleteList.Should().NotBeNull();
            deleteList.Count.Should().Be(0);

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenAgentAssignment_WhenDuplicatedAdded_ShouldSuccessAsUpdate()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<TraceRecord> container = await watcherRepository.Container.Create<TraceRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = new TraceRecord
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                AgentId = "agent",
                TargetId = "target",
                Url = "http://localhost",
                HttpStatusCode = HttpStatusCode.BadRequest,
                Body = "this is the body",
                TargetState = TargetState.Ok
            };

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<TraceRecord> activeList = await container.Search("select * from ROOT");
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
        public async Task GivenMultipleTraces_WhenRoundTrip_ShouldSuccess()
        {
            const int max = 10;

            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<TraceRecord> container = await watcherRepository.Container.Create<TraceRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            IReadOnlyList<TraceRecord> records = Enumerable.Range(0, max)
                .Select(x => new TraceRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    AgentId = "agent",
                    TargetId = "target",
                    Url = "http://localhost",
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Body = "this is the body",
                    TargetState = TargetState.Ok
                })
                .ToArray();

            var eTags = new List<ETag>();
            foreach (TraceRecord item in records)
            {
                ETag eTag = await container.Set(item);
                eTag.Should().NotBeNull();
                eTag.Value.Should().NotBeEmpty();

                eTags.Add(eTag);
            }

            // Act - list records
            IReadOnlyList<TraceRecord> activeList = await container.Search("select * from ROOT");
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
                IReadOnlyList<TraceRecord> queryList = await container.Search($"select * from ROOT r where r.id = \"{record.Id}\"");
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
