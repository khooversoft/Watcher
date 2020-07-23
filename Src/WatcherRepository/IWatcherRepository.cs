using System.Threading;
using System.Threading.Tasks;

namespace WatcherRepository
{
    public interface IWatcherRepository
    {
        IRepositoryContainer Container { get; }
        IRepositoryDatabase Database { get; }

        Task InitializeEnvironment(CancellationToken token = default);
    }
}