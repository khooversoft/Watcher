﻿using System.Threading;
using System.Threading.Tasks;

namespace WatcherSdk.Repository
{
    public interface IWatcherRepository
    {
        IRepositoryContainer Container { get; }
        IRepositoryDatabase Database { get; }

        Task InitializeContainers(CancellationToken token = default);
    }
}