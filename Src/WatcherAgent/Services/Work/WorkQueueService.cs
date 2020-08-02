using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WatcherAgent.Services.Work
{
    public class WorkQueueService : IWorkQueueService
    {
        private readonly ILogger<WorkQueueService> _logger;
        private readonly ActionBlock<(string workId, Func<CancellationToken, Task>)> _workQueue;

        public WorkQueueService(ILogger<WorkQueueService> logger)
        {
            _logger = logger;
            _workQueue = new ActionBlock<(string workId, Func<CancellationToken, Task>)>(Execute);
        }

        public void Post(string workId, Func<CancellationToken, Task> actionAsync)
        {
            _logger.LogTrace($"{nameof(Post)}: Enqueue action, workId={workId}");
            _workQueue.Post((workId, actionAsync));
        }

        public async Task SendAsync(string workId, Func<CancellationToken, Task> actionAsync)
        {
            _logger.LogTrace($"{nameof(SendAsync)}: Enqueue action, workdId={workId}");
            await _workQueue.SendAsync((workId, actionAsync));
        }

        private async Task Execute((string workId, Func<CancellationToken, Task> actionAsync) workItem)
        {
            _logger.LogTrace($"{nameof(Execute)}: Executing {workItem.workId}");
            using CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await workItem.actionAsync(tokenSource.Token);
        }
    }
}
