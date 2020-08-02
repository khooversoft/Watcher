using System;
using System.Threading;
using System.Threading.Tasks;

namespace WatcherAgent.Services.Work
{
    public interface IWorkQueueService
    {
        void Post(string workId, Func<CancellationToken, Task> actionAsync);
        Task SendAsync(string workId, Func<CancellationToken, Task> actionAsync);
    }
}