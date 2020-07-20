using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
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
            _loggerFactory = new TestLoggerBuilder().Build();
            _watcherOption = new TestOptionBuilder().Build();
        }

        [TestMethod]
        public async Task GivenDatabase_WhenCreatedAndDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName);

            // Act
            await watcherRepository.CreateDatabase(_databaseName);

            // Assert
            (await watcherRepository.DeleteDatabase(_databaseName)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenContainer_WhenCreatedAndDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName);

            // Act
            WatcherContainer<TestItem> container = await watcherRepository.CreateContainer<TestItem>(_databaseName, _containerName, partitionKey: "/header");
            container.Should().NotBeNull();

            // Assert
            (await watcherRepository.DeleteContainer(_databaseName, _containerName)).Should().BeTrue();
            (await watcherRepository.DeleteDatabase(_databaseName)).Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenContainer_WhenCreatedAndDatabaseDeleted_ShouldSucceed()
        {
            // Arrange
            IWatcherRepository watcherRepository = new WatcherRepository(_watcherOption, _loggerFactory);
            await watcherRepository.DeleteDatabase(_databaseName);

            // Act
            WatcherContainer<TestItem> container = await watcherRepository.CreateContainer<TestItem>(_databaseName, _containerName, partitionKey: "/header");
            container.Should().NotBeNull();

            // Assert
            (await watcherRepository.DeleteDatabase(_databaseName)).Should().BeTrue();
        }

        private class TestItem { }
    }
}
