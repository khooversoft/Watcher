using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace WatcherRepository
{
    public class WatcherRepository : IWatcherRepository
    {
        private const string _recordText = "Record";
        private readonly string _connectionString;
        private readonly IWatcherOption _watcherOption;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<WatcherRepository> _logger;
        private readonly CosmosClient _cosmosClient;

        public WatcherRepository(IWatcherOption watcherOption, ILoggerFactory loggerFactory)
        {
            watcherOption.VerifyNotNull(nameof(watcherOption)).Verify();
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _connectionString = watcherOption.GetResolvedConnectionString();
            _watcherOption = watcherOption;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<WatcherRepository>();
            _cosmosClient = new CosmosClient(_connectionString);
        }

        public async Task CreateDatabase(string databaseName, CancellationToken token = default)
        {
            databaseName.VerifyNotEmpty(nameof(databaseName));

            _logger.LogTrace($"{nameof(CreateDatabase)}: Create databaseName={databaseName}");
            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: token);
        }

        public async Task<bool> DeleteDatabase(string databaseName, CancellationToken token = default)
        {
            databaseName.VerifyNotEmpty(nameof(databaseName));
            Database database = _cosmosClient.GetDatabase(databaseName);

            _logger.LogTrace($"{nameof(DeleteDatabase)}: DatabaseName={databaseName}");

            try
            {
                DatabaseResponse response = await database.DeleteAsync(cancellationToken: token);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            return true;
        }

        public Task<WatcherContainer<T>> CreateContainer<T>(string databaseName, int? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class
        {
            string containerName = typeof(T).Name
                .Func(x => x.EndsWith(_recordText) ? x.Substring(0, x.Length - _recordText.Length) : x)
                .Func(x => _watcherOption.CreateContainerName(x));

            return CreateContainer<T>(databaseName, containerName, defaultTimeToLive, partitionKey, token);
        }

        public async Task<WatcherContainer<T>> CreateContainer<T>(string databaseName, string containerName, int? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class
        {
            databaseName.VerifyNotEmpty(nameof(databaseName));
            containerName.VerifyNotEmpty(nameof(containerName));
            partitionKey = partitionKey.ToNullIfEmpty() ?? WatcherContainer<T>.DefaultPartitionKey;

            _logger.LogTrace($"{nameof(CreateContainer)}: Create databaseName={databaseName}");
            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: token);

            _logger.LogTrace($"{nameof(CreateContainer)}: Create container, DatabaseName={databaseName}, ContainerName={containerName}, PartitionKey={partitionKey}, DefaultTTL={defaultTimeToLive}");
            ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPath = partitionKey,
                DefaultTimeToLive = defaultTimeToLive,
            }, cancellationToken: token);

            return new WatcherContainer<T>(containerResponse.Container, _loggerFactory.CreateLogger<WatcherContainer<T>>());
        }

        public async Task<bool> DeleteContainer(string databaseName, string containerName, CancellationToken token = default)
        {
            databaseName.VerifyNotEmpty(nameof(databaseName));
            containerName.VerifyNotEmpty(nameof(containerName));

            Database database = _cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);

            _logger.LogTrace($"{nameof(DeleteContainer)}: Delete container, , DatabaseName={databaseName}, ContainerName={containerName}");
            await container.DeleteContainerAsync(cancellationToken: token);

            return true;
        }
    }
}
