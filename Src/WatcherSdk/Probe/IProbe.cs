using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherSdk.Probe
{
    public interface IProbe
    {
        Task<bool> Ping(CancellationToken token);
    }
}
