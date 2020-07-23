using System.Threading;
using System.Threading.Tasks;

namespace WatcherRepository
{
    public interface IRepositoryDatabase
    {
        Task Create(string databaseName, CancellationToken token = default);
        Task<bool> Delete(string databaseName, CancellationToken token = default);
    }
}