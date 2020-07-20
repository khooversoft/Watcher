using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Extensions;
using WatcherRepository.Models;
using System.Net;

namespace WatcherRepository
{
    public class WatcherContainer<T>
        where T : class
    {
        private readonly Container _container;
        private readonly ILogger<WatcherContainer<T>> _logger;

        private readonly HashSet<int> _validStatusCode = new HashSet<int>(new[]
        {
            (int)HttpStatusCode.OK,
            (int)HttpStatusCode.Created,
            (int)HttpStatusCode.NotFound,
            (int)HttpStatusCode.NoContent,
        });

        public WatcherContainer(Container container, ILogger<WatcherContainer<T>> logger)
        {
            container.VerifyNotNull(nameof(container));

            _container = container;
            _logger = logger;
        }

        public static string DefaultPartitionKey = "/id";

        public string ContainerName => _container.Id;

        public async Task<ETag> Set(T item, string? eTag = null, CancellationToken token = default)
        {
            item.VerifyNotNull(nameof(item));

            _logger.LogTrace($"{nameof(Set)}: {nameof(_container.UpsertItemAsync)} item, ");

            var option = new ItemRequestOptions
            {
                IfMatchEtag = eTag
            };

            ItemResponse<T> itemResponse = await _container.UpsertItemAsync(item, requestOptions: option, cancellationToken: token);

            itemResponse.VerifyAssert(x => IsValid(x.StatusCode), x =>
            {
                string msg = $"{nameof(Set)}: Failed to {nameof(_container.UpsertItemAsync)}, StatusCode={x.StatusCode}";
                _logger.LogError(msg);
                return msg;
            });

            return (ETag)itemResponse.ETag;
        }

        public async Task<T?> Get(string id, string? partitionKey = null, CancellationToken token = default)
        {
            id.VerifyNotEmpty(nameof(id));

            try
            {
                ItemResponse<T> itemResponse = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey ?? id), cancellationToken: token);

                itemResponse.VerifyAssert(x => IsValid(x.StatusCode), x =>
                {
                    string msg = $"{nameof(Get)}: Failed to {nameof(_container.UpsertItemAsync)} in getting id={id}, StatusCode={x.StatusCode}";
                    _logger.LogError(msg);
                    return msg;
                });

                return itemResponse.StatusCode == HttpStatusCode.OK ? itemResponse.Resource : default;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError($"{nameof(Get)}: Record does not exit, id={id}");
                return default;
            }
        }

        public async Task<bool> Delete(string id, string? eTag = null, string? partitionKey = null, CancellationToken token = default)
        {
            id.VerifyNotEmpty(nameof(id));

            _logger.LogTrace($"{nameof(Delete)}: Deleting {id}, eTag={eTag}");

            var option = new ItemRequestOptions
            {
                IfMatchEtag = eTag
            };

            try
            {
                ItemResponse<T> itemResponse = await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey ?? id), requestOptions: option, cancellationToken: token);

                itemResponse.VerifyAssert(x => IsValid(x.StatusCode), x =>
                {
                    string msg = $"{nameof(Delete)}: Failed to {nameof(_container.UpsertItemAsync)}, StatusCode={x.StatusCode}";
                    _logger.LogError(msg);
                    return msg;
                });

                return itemResponse.StatusCode == HttpStatusCode.NoContent ? true : false;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError($"{nameof(Get)}: Record does not exit, id={id}");
                return default;
            }
        }

        public async Task<IReadOnlyList<T>> Search(string sqlQuery, CancellationToken token = default)
        {
            sqlQuery.VerifyNotEmpty(nameof(sqlQuery));

            var list = new List<T>();

            _logger.LogTrace($"{nameof(Search)}: Query={sqlQuery}");

            var queryDefinition = new QueryDefinition(sqlQuery);

            using FeedIterator<T> feedIterator = _container.GetItemQueryIterator<T>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                foreach (var item in await feedIterator.ReadNextAsync())
                {
                    list.Add(item);
                }
            }

            _logger.LogTrace($"{nameof(Search)}: Query={sqlQuery}, RecordCount={list.Count}");

            return list;
        }

        private bool IsValid(HttpStatusCode httpStatusCode) => _validStatusCode.Contains((int)httpStatusCode);
    }
}
