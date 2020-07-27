using System;
using System.Collections.Generic;

namespace Watcher.Cosmos.Repository.Application
{
    public interface ICosmosWatcherOption
    {
        string AccountKey { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        TimeSpan HeartbeatFrequency { get; set; }
        TimeSpan OfflineTolerance { get; set; }
        TimeSpan TraceTraceTTL { get; set; }

        IReadOnlyList<KeyValuePair<string, string>> GetProperties();
        string GetResolvedConnectionString();
        string Resolve(string value);
        void Verify();
    }
}