using System.Threading;
using System.Threading.Tasks;

namespace WatcherSdk.Repository
{
    public interface IRepositoryDatabase
    {
        Task Create(string databaseName, CancellationToken token = default);
        Task<bool> Delete(string databaseName, CancellationToken token = default);
    }
}