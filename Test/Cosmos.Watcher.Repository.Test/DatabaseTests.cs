using Cosmos.Watcher.Repository.Test.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Xunit;

namespace Cosmos.Watcher.Repository.Test
{
    public class DatabaseTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private ICosmosWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase";
        private const string _containerName = "testDatabase.testContainer";

        public DatabaseTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"databaseName={_databaseName}");
        }

        [Fact]
        public async Task GivenDatabase_WhenCreatedAndDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);

            // Act
            await watcherRepository.Database.Create(_databaseName);

            // Assert
            (await watcherRepository.Database.Delete(_databaseName)).Should().BeTrue();
        }

        [Fact]
        public async Task GivenContainer_WhenCreatedAndDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);

            // Act
            IRecordContainer<TestItem> container = await watcherRepository.Container.Create<TestItem>(_containerName, partitionKey: "/header");
            container.Should().NotBeNull();

            // Assert
            (await watcherRepository.Container.Delete(_containerName)).Should().BeTrue();
            (await watcherRepository.Database.Delete(_databaseName)).Should().BeTrue();
        }

        [Fact]
        public async Task GivenContainer_WhenCreatedAndDatabaseDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new CosmosWatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);

            // Act
            IRecordContainer<TestItem> container = await watcherRepository.Container.Create<TestItem>(_containerName, partitionKey: "/header");
            container.Should().NotBeNull();

            // Assert
            (await watcherRepository.Database.Delete(_databaseName)).Should().BeTrue();
        }

        private class TestItem : IRecord
        {
            public string Id => throw new System.NotImplementedException();

            public void Prepare() => throw new System.NotImplementedException();
        }
    }
}
