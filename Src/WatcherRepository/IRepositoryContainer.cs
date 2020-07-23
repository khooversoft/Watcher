using System;
using System.Threading;
using System.Threading.Tasks;
using WatcherRepository.Records;

namespace WatcherRepository
{
    public interface IRepositoryContainer
    {
        Task<RecordContainer<T>> Create<T>(TimeSpan? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class, IRecord;
        Task<RecordContainer<T>> Create<T>(string containerName, TimeSpan? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class, IRecord;
        Task<bool> Delete(string containerName, CancellationToken token = default);
        RecordContainer<T> Get<T>() where T : class, IRecord;
        RecordContainer<T> Get<T>(string containerName) where T : class, IRecord;
    }
}