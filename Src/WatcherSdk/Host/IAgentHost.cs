using System.Threading;
using System.Threading.Tasks;

namespace WatcherSdk.Host
{
    public interface IAgentHost
    {
        Task Start(CancellationToken token);

        Task Stop();
    }
}