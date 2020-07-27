using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherSdk.Probe
{
    public interface IProbeFactory
    {
        IProbe Create(TargetRecord targetRecord, string agentId, IRecordContainer<TraceRecord>? traceContainer);
    }
}