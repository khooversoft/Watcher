using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WatcherSdk.Models;
using WatcherSdk.Records;

namespace WatcherSdk.Repository
{
    public interface IRecordContainer<T> where T : class, IRecord
    {
        string ContainerName { get; }

        Task<bool> Delete(string id, string? eTag = null, string? partitionKey = null, CancellationToken token = default);
        Task<bool> Exist(string id, CancellationToken token = default);
        Task<Record<T>?> Get(string id, string? partitionKey = null, CancellationToken token = default);
        Task<IReadOnlyList<T>> ListAll(CancellationToken token = default);
        Task<IReadOnlyList<T>> Search(string sqlQuery, CancellationToken token = default);
        Task<ETag> Set(Record<T> record, CancellationToken token = default);
        Task<ETag> Set(T item, CancellationToken token = default);
    }
}