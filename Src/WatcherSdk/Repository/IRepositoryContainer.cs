using System;
using System.Threading;
using System.Threading.Tasks;
using WatcherSdk.Records;

namespace WatcherSdk.Repository
{
    public interface IRepositoryContainer
    {
        Task<IRecordContainer<T>> Create<T>(TimeSpan? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class, IRecord;
        Task<IRecordContainer<T>> Create<T>(string containerName, TimeSpan? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class, IRecord;
        Task<bool> Delete(string containerName, CancellationToken token = default);
        IRecordContainer<T> Get<T>() where T : class, IRecord;
        IRecordContainer<T> Get<T>(string containerName) where T : class, IRecord;
    }
}