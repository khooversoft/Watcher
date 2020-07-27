using System.Threading;
using System.Threading.Tasks;

namespace WatcherSdk.Agent
{
    public interface IAgentHost
    {
        Task Start(CancellationToken token);
    }
}