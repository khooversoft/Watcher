using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WatcherSdk.Records;

namespace WatcherAgent.Services.AgentAssignment
{
    internal interface IAgentAssignmentService
    {
        IReadOnlyList<TargetRecord> Items { get; }

        Task Read(CancellationToken token);

        Task Sync(CancellationToken token);
    }
}