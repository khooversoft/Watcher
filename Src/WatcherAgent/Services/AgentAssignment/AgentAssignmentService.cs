using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WatcherAgent.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherAgent.Services.AgentAssignment
{
    internal class AgentAssignmentService : IAgentAssignmentService
    {
        private readonly IOption _option;
        private readonly IRecordContainer<AgentAssignmentRecord> _assignmentContainer;
        private readonly IRecordContainer<TargetRecord> _targetContainer;
        private readonly ILogger<AgentAssignmentService> _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private DateTime? _validUntil;
        private TimeSpan _syncFrequency = TimeSpan.FromSeconds(15);
        private IReadOnlyList<TargetRecord>? _currentList;

        public AgentAssignmentService(IOption option, IRecordContainer<AgentAssignmentRecord> assignmentContainer, IRecordContainer<TargetRecord> targetContainer, ILogger<AgentAssignmentService> logger)
        {
            _option = option;
            _assignmentContainer = assignmentContainer;
            _targetContainer = targetContainer;
            _logger = logger;
        }

        public IReadOnlyList<TargetRecord> Items => _currentList ?? Array.Empty<TargetRecord>();

        public Task Read(CancellationToken token) => ReadList(true, token);

        public Task Sync(CancellationToken token) => ReadList(false, token);

        private async Task ReadList(bool force, CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Read)}: Reading");

            await _lock.WaitAsync(token);
            try
            {
                if (!force && _currentList != null && _validUntil != null && _validUntil > DateTime.Now) return;

                _validUntil = DateTime.Now + _syncFrequency;

                IReadOnlyList<AgentAssignmentRecord> assignmentList = _assignmentContainer.GetAssignmentsForAgent(_option.AgentId, _logger, token);

                IReadOnlyList<TargetRecord> targetList = await _targetContainer.ListAll(token):

                _currentList = assignmentList;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
