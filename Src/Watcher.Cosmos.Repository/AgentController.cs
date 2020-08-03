using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace Watcher.Cosmos.Repository
{
    public class AgentController : IAgentController
    {
        private readonly ICosmosWatcherOption _option;
        private readonly IWatcherRepository _watcherRepository;
        private readonly ILogger<AgentController> _logger;
        private readonly IRecordContainer<AgentRecord> _agentContainer;
        private readonly IRecordContainer<TargetRecord> _targetContainer;

        public AgentController(ICosmosWatcherOption option, IWatcherRepository watcherRepository, ILogger<AgentController> logger)
        {
            option.VerifyNotNull(nameof(option));
            watcherRepository.VerifyNotNull(nameof(watcherRepository));
            logger.VerifyNotNull(nameof(logger));

            _option = option;
            _watcherRepository = watcherRepository;
            _logger = logger;

            _agentContainer = _watcherRepository.Container.Get<AgentRecord>();
            _targetContainer = _watcherRepository.Container.Get<TargetRecord>();
        }

        public async Task Register(AgentRecord agentRecord, CancellationToken token = default)
        {
            agentRecord.VerifyNotNull(nameof(agentRecord));
            _logger.LogTrace($"{nameof(Register)}: agentId={agentRecord.Id}, state={agentRecord.State}");

            await _agentContainer.Set(agentRecord, token: token);
        }

        public async Task UnRegister(string agentName, CancellationToken token = default)
        {
            agentName.VerifyNotEmpty(nameof(agentName)).ToLowerInvariant();
            _logger.LogTrace($"{nameof(UnRegister)}: agentId={agentName}");

            await _agentContainer.Delete(agentName, token: token);

            _logger.LogTrace($"{nameof(UnRegister)}: Remove assignments for agent id {agentName}");
            IReadOnlyList<TargetRecord> agentAssignments = await _targetContainer.GetAssignments(agentName, _logger, token);

            foreach (var item in agentAssignments)
            {
                await _targetContainer.Delete(item.Id);
            }
        }

        public async Task<bool> SetState(string agentName, AgentState agentState, CancellationToken token = default)
        {
            agentName.VerifyNotEmpty(nameof(agentName));
            _logger.LogTrace($"{nameof(SetState)}: agentId={agentName}, state={agentState}");

            Record<AgentRecord>? agentRecord = await _agentContainer.Get(agentName, token: token);
            if (agentRecord == null) return false;

            agentRecord.Value.State = agentState;
            await _agentContainer.Set(agentRecord);
            return true;
        }

        /// <summary>
        /// Load balance assignments (agents -> targets)
        /// 
        /// Without a master node, each agent will help with the load balancing.  There are edge cases where a target will be
        /// monitoring by more then 1 agent.
        /// 
        /// The process for spinning up an agent is 2 steps, register and get assignments.
        /// 
        /// The processing rules for load balancing are...
        ///
        /// (1) Get active agents, filter out agents that are out of tolerance based on timestamps for heart beat
        /// (2) Get active targets, filter out based on "enabled"
        /// (3) Get assignments and load into dictionary, for editing
        /// (4) Remove assignments that are assigned to non-active agents
        /// (5) Test to see if a re-balance is required (assignments are too out of balance, pass a threshold of .2, It true, remove all assignments in cache to force re-balance
        /// (6) Add new assignments for targets, over active agents
        /// (7) Compare to current assignment in store with internal dictionary and adjust the store to make equal (sync, with dictionary being master)
        /// 
        /// </summary>
        /// <param name="token">token</param>
        /// <returns>Task</returns>
        public async Task LoadBalanceAssignments(CancellationToken token = default)
        {
            _logger.LogTrace($"{nameof(LoadBalanceAssignments)}: Running");

            IReadOnlyList<AgentRecord> activeAgents = await GetActiveAgents(token);
            if (activeAgents.Count == 0) return;

            IReadOnlyList<TargetRecord> activeTargets = await GetActiveTargets(token);

            // Remove assignments and refresh
            _logger.LogTrace($"{nameof(LoadBalanceAssignments)}: Delete assignments where not assigned to active agents");
            RemoveAssignmentInvalidAgentAssignment(activeTargets, activeAgents);

            // Re-balance if required if assignment counts are out of tolerance
            RebalanceIfRequired(activeTargets, activeAgents);

            // Assign un-assigned targets to agents
            AssignTargetsToAgents(activeTargets, activeAgents);

            // Update the DB with assignment delta
            await UpdateAssignments(activeTargets, token);
        }

        private async Task<IReadOnlyList<AgentRecord>> GetActiveAgents(CancellationToken token) =>
            (await _agentContainer.ListAll(token))
                .Where(x => x.IsAgentRunning(_option.OfflineTolerance))
                .ToArray();

        private async Task<IReadOnlyList<TargetRecord>> GetActiveTargets(CancellationToken token) =>
            (await _targetContainer.ListAll(token))
                .Where(x => x.Enabled)
                .ToArray();

        private void RemoveAssignmentInvalidAgentAssignment(IEnumerable<TargetRecord> activeTargets, IEnumerable<AgentRecord> agentRecords) => activeTargets
               .Where(x => !agentRecords.Any(y => y.Id == x.AssignedAgentId))
               .ForEach(x => x.AssignedAgentId = null);

        private int AgentAssignmentCount(AgentRecord agentRecord, IReadOnlyList<TargetRecord> activeAssignment) => activeAssignment.Count(x => x.AssignedAgentId == agentRecord.Id);

        private void RebalanceIfRequired(IReadOnlyList<TargetRecord> activeTargets, IReadOnlyList<AgentRecord> activeAgents)
        {
            _logger.LogTrace($"{nameof(RebalanceIfRequired)}: Check");

            int agentGoal = Math.Max(activeTargets.Count / activeAgents.Count, 1);

            bool overLimit = activeAgents
                .Select(x => AgentAssignmentCount(x, activeTargets))
                .Any(x => agentGoal - x > (int)(agentGoal * .8));

            if (!overLimit) return;

            _logger.LogInformation($"{nameof(RebalanceIfRequired)}: Forcing re-balance");

            activeTargets
                .ForEach(x => x.AssignedAgentId = null);
        }

        private void AssignTargetsToAgents(IReadOnlyList<TargetRecord> activeTargets, IReadOnlyList<AgentRecord> activeAgents)
        {
            _logger.LogTrace($"{nameof(AssignTargetsToAgents)}: Assign un-assigned targets to agents");

            var newTargets = activeTargets
                .Where(x => x.AssignedAgentId == null)
                .ToArray();

            var newTargetsQueue = new Queue<TargetRecord>(newTargets);

            while (newTargetsQueue.TryDequeue(out TargetRecord targetRecord))
            {
                AgentRecord agent = activeAgents
                    .Select(x => (Agent: x, Count: AgentAssignmentCount(x, activeTargets)))
                    .OrderBy(x => x.Count)
                    .Select(x => x.Agent)
                    .First();

                targetRecord.AssignedAgentId = agent.Id;
            }
        }

        private Task UpdateAssignments(IReadOnlyList<TargetRecord> activeTargets, CancellationToken token)
        {
            _logger.LogTrace($"{nameof(UpdateAssignments)}: Update assignments based on load balancing work");

            Task[] tasks = activeTargets
                .Select(x => _targetContainer.Set(x, token))
                .ToArray();

            return Task.WhenAll(tasks);
        }
    }
}

