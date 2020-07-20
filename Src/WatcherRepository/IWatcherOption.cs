using System;

namespace WatcherRepository
{
    public interface IWatcherOption
    {
        string AccountKey { get; }
        string ConnectionString { get; }
        string Environment { get; }
        string Tenant { get; }
        TimeSpan TraceTTL { get; }

        string CreateContainerName(string subject);
        string GetResolvedConnectionString();
        void Verify();
    }
}