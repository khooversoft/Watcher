﻿using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Xunit;

namespace Cosmos.Watcher.Repository.Test.RecordTests
{
    public class TargetRecordTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private ICosmosWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase.assignment";

        public TargetRecordTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"DatabaseName={_databaseName}");
        }

        [Fact]
        public async Task GivenAgentAssignment_WhenRoundTrip_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<TargetRecord> container = await watcherRepository.Container.Create<TargetRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = CreateTargetRecord(1);

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<TargetRecord> activeList = await container.Search("select * from ROOT");
            activeList.Should().NotBeNull();
            activeList.Count.Should().Be(1);

            // Act - Read
            Record<TargetRecord>? read = await container.Get(record.Id);
            read.Should().NotBeNull();
            (read!.Value == record).Should().BeTrue();

            // Act - Delete
            (await container.Delete(record.Id, etag)).Should().BeTrue();

            IReadOnlyList<TargetRecord> deleteList = await container.Search("select * from ROOT");
            deleteList.Should().NotBeNull();
            deleteList.Count.Should().Be(0);

            // Clean up
            (await watcherRepository.Database.Delete(_databaseName, CancellationToken.None)).Should().BeTrue();
        }

        [Fact]
        public async Task GivenAgentAssignment_WhenDuplicatedAdded_ShouldSuccessAsUpdate()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<TargetRecord> container = await watcherRepository.Container.Create<TargetRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            var record = CreateTargetRecord(1);

            ETag etag = await container.Set(record);
            etag.Should().NotBeNull();
            etag.Value.Should().NotBeEmpty();

            IReadOnlyList<TargetRecord> activeList = await container.Search("select * from ROOT");
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

        [Fact]
        public async Task GivenMultipleTargets_WhenRoundTrip_ShouldSuccess()
        {
            const int max = 10;

            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName, CancellationToken.None);

            IRecordContainer<TargetRecord> container = await watcherRepository.Container.Create<TargetRecord>(_databaseName);
            container.Should().NotBeNull();

            // Act - Write
            IReadOnlyList<TargetRecord> records = Enumerable.Range(0, max)
                .Select((x, i) => CreateTargetRecord(i))
                .ToArray();

            var eTags = new List<ETag>();
            foreach (TargetRecord item in records)
            {
                ETag eTag = await container.Set(item);
                eTag.Should().NotBeNull();
                eTag.Value.Should().NotBeEmpty();

                eTags.Add(eTag);
            }

            // Act - list records
            IReadOnlyList<TargetRecord> activeList = await container.Search("select * from ROOT");
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
                IReadOnlyList<TargetRecord> queryList = await container.Search($"select * from ROOT r where r.id = \"{record.Id}\"");
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

        private TargetRecord CreateTargetRecord(int index) =>
            new TargetRecord
            {
                Id = $"target.{index}",
                Description = "Target description",
                ReadyUrl = "http://localhost:3003/predict",
                StatusCodeMaps = new List<StatusCodeMap>
                {
                    new StatusCodeMap { HttpStatusCode = HttpStatusCode.OK, State = TargetState.Ok},
                    new StatusCodeMap { HttpStatusCode = HttpStatusCode.NotFound, State = TargetState.Error},
                },
                BodyElementMaps = new List<BodyElementMap>
                {
                    new BodyElementMap { Path = "body/status", CompareTo = "Ok", State = TargetState.Ok },
                    new BodyElementMap { Path = "body/status", CompareTo = "Error", State = TargetState.Error },
                },
                TargetType = "rest",
                Enabled = true,
                FrequencyInSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds,
            };
    }
}
