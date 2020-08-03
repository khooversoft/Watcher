using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WatcherSdk.Models;
using WatcherSdk.Records;

namespace Watcher.Cosmos.Repository
{
    public interface IAgentController
    {
        Task LoadBalanceAssignments(CancellationToken token = default);
        Task Register(AgentRecord agentRecord, CancellationToken token = default);
        Task<bool> SetState(string agentName, AgentState agentState, CancellationToken token = default);
        Task UnRegister(string agentName, CancellationToken token = default);
    }
}