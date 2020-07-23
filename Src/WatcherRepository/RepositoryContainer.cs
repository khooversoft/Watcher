using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using WatcherRepository.Application;
using WatcherRepository.Records;

namespace WatcherRepository
{
    public class RepositoryContainer : IRepositoryContainer
    {
        private const string _recordText = "Record";
        private readonly IWatcherOption _watcherOption;
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<RepositoryContainer> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public RepositoryContainer(IWatcherOption watcherOption, CosmosClient cosmosClient, ILoggerFactory loggerFactory, ILogger<RepositoryContainer> logger)
        {
            watcherOption.VerifyNotNull(nameof(watcherOption)).Verify();
            cosmosClient.VerifyNotNull(nameof(cosmosClient));
            loggerFactory.VerifyNotNull(nameof(loggerFactory));
            logger.VerifyNotNull(nameof(logger));

            _watcherOption = watcherOption;
            _cosmosClient = cosmosClient;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public Task<RecordContainer<T>> Create<T>(TimeSpan? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class, IRecord =>
            Create<T>(GetContainerName<T>(), defaultTimeToLive, partitionKey, token);

        public async Task<RecordContainer<T>> Create<T>(string containerName, TimeSpan? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class, IRecord
        {
            containerName.VerifyNotEmpty(nameof(containerName));
            partitionKey = partitionKey.ToNullIfEmpty() ?? Constants.DefaultPartitionKey;

            _logger.LogTrace($"{nameof(Create)}: Create databaseName={_watcherOption.DatabaseName}");
            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_watcherOption.DatabaseName, cancellationToken: token);

            _logger.LogTrace($"{nameof(Create)}: Create container, DatabaseName={_watcherOption.DatabaseName}, ContainerName={containerName}, PartitionKey={partitionKey}, DefaultTTL={defaultTimeToLive}");
            ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPath = partitionKey,
                DefaultTimeToLive = (int?)defaultTimeToLive?.TotalSeconds,
            }, cancellationToken: token);

            return new RecordContainer<T>(containerResponse.Container, _loggerFactory.CreateLogger<T>());
        }

        public RecordContainer<T> Get<T>() where T : class, IRecord => Get<T>(GetContainerName<T>());

        public RecordContainer<T> Get<T>(string containerName) where T : class, IRecord
        {
            containerName.VerifyNotEmpty(nameof(containerName));

            Database database = _cosmosClient.GetDatabase(_watcherOption.DatabaseName);
            Container container = database.GetContainer(containerName);

            return new RecordContainer<T>(container, _loggerFactory.CreateLogger<T>());
        }

        public async Task<bool> Delete(string containerName, CancellationToken token = default)
        {
            containerName.VerifyNotEmpty(nameof(containerName));

            Database database = _cosmosClient.GetDatabase(_watcherOption.DatabaseName);
            Container container = database.GetContainer(containerName);

            _logger.LogTrace($"{nameof(Delete)}: Delete container, , DatabaseName={_watcherOption.DatabaseName}, ContainerName={containerName}");
            await container.DeleteContainerAsync(cancellationToken: token);

            return true;
        }

        private string GetContainerName<T>() => typeof(T).Name
            .Func(x => x.EndsWith(_recordText) ? x.Substring(0, x.Length - _recordText.Length) : x);
    }
}
