using System.Threading;
using System.Threading.Tasks;

namespace WatcherSdk.Agent
{
    public interface IMonitoringJob
    {
        Task Start(CancellationToken token);
    }
}