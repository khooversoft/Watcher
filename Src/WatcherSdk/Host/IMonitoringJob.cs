using System.Threading;
using System.Threading.Tasks;

namespace WatcherSdk.Host
{
    public interface IMonitoringJob
    {
        Task Run(CancellationToken token);
    }
}