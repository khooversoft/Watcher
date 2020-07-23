using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using WatcherRepository.Application;
using WatcherRepository.Records;
using WatcherRepository.Test.Application;

namespace WatcherRepository.Test
{
    [TestClass]
    public class DatabaseTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerBuilder().Build();
        private IWatcherOption _watcherOption = new TestOptionBuilder().Build();
        private const string _databaseName = "testDatabase";
        private const string _containerName = "testDatabase.testContainer";

        public DatabaseTests()
        {
            _watcherOption = new TestOptionBuilder().Build($"databaseName={_databaseName}");
        }

        [TestMethod]
        public async Task GivenDatabase_WhenCreatedAndDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);

            // Act
            await watcherRepository.Database.Create(_databaseName);

            // Assert
            (await watcherRepository.Database.Delete(_databaseName)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenContainer_WhenCreatedAndDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);

            // Act
            RecordContainer<TestItem> container = await watcherRepository.Container.Create<TestItem>(_containerName, partitionKey: "/header");
            container.Should().NotBeNull();

            // Assert
            (await watcherRepository.Container.Delete(_containerName)).Should().BeTrue();
            (await watcherRepository.Database.Delete(_databaseName)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenContainer_WhenCreatedAndDatabaseDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.Database.Delete(_databaseName);

            // Act
            RecordContainer<TestItem> container = await watcherRepository.Container.Create<TestItem>(_containerName, partitionKey: "/header");
            container.Should().NotBeNull();

            // Assert
            (await watcherRepository.Database.Delete(_databaseName)).Should().BeTrue();
        }

        private class TestItem : IRecord
        {
            public void Prepare() => throw new System.NotImplementedException();
        }
    }
}
