using System.Threading;
using System.Threading.Tasks;

namespace WatcherRepository
{
    public interface IWatcherRepository
    {
        Task<WatcherContainer<T>> CreateContainer<T>(string databaseName, int? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class;
        Task<WatcherContainer<T>> CreateContainer<T>(string databaseName, string containerName, int? defaultTimeToLive = null, string? partitionKey = null, CancellationToken token = default) where T : class;
        Task CreateDatabase(string databaseName, CancellationToken token = default);
        Task<bool> DeleteContainer(string databaseName, string containerName, CancellationToken token = default);
        Task<bool> DeleteDatabase(string databaseName, CancellationToken token = default);
    }
}