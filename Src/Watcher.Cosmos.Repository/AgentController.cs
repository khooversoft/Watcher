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
        private readonly IRecordContainer<AgentAssignmentRecord> _agentAssignmentContainer;
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
            _agentAssignmentContainer = _watcherRepository.Container.Get<AgentAssignmentRecord>();
            _targetContainer = _watcherRepository.Container.Get<TargetRecord>();
        }

        public async Task Register(AgentRecord agentRecord, CancellationToken token = default)
        {
            agentRecord.VerifyNotNull(nameof(agentRecord));
            _logger.LogTrace($"{nameof(Register)}: Agent id {agentRecord.Id}, state={agentRecord.State}");

            await _agentContainer.Set(agentRecord, token: token);
        }

        public async Task UnRegister(string agentName, CancellationToken token = default)
        {
            agentName.VerifyNotEmpty(nameof(agentName)).ToLowerInvariant();
            _logger.LogTrace($"{nameof(UnRegister)}: Agent id {agentName}");

            await _agentContainer.Delete(agentName, token: token);

            _logger.LogTrace($"{nameof(UnRegister)}: Remove assignments for agent id {agentName}");
            IReadOnlyList<AgentAssignmentRecord> assignments = await _agentAssignmentContainer.Search($"select * from ROOT r where r.AgentId = \"{agentName}\"");

            foreach (var item in assignments)
            {
                await _agentAssignmentContainer.Delete(item.Id);
            }
        }

        public async Task<bool> SetState(string agentName, AgentState agentState, CancellationToken token = default)
        {
            agentName.VerifyNotEmpty(nameof(agentName));
            _logger.LogTrace($"{nameof(SetState)}: Agent id {agentName}, state={agentState}");

            Record<AgentRecord>? agentRecord = await _agentContainer.Get(agentName, token: token);
            if (agentRecord == null) return false;

            agentRecord.Value.State = agentState;
            await _agentContainer.Set(agentRecord);
            return true;
        }

        public async Task<IReadOnlyList<TargetRecord>> GetAssignments(string agentName, CancellationToken token = default)
        {
            var assignedTargets = (await _agentAssignmentContainer.ListAll(token))
                .Where(x => x.AgentId == agentName)
                .Join(await GetActiveTargets(token), x => x.TargetId, x => x.Id, (o, i) => i)
                .ToArray();

            _logger.LogTrace($"{nameof(GetAssignments)}: Agent {agentName} is assigned {string.Join(", ", assignedTargets.Select(x => x.Id))}");

            return assignedTargets;
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
            IDictionary<string, AgentAssignmentRecord> activeAssignments = (await _agentAssignmentContainer.ListAll(token)).ToDictionary(x => x.Id);

            // Remove assignments and refresh
            _logger.LogTrace($"{nameof(GetAssignments)}: Delete assignments where not assigned to active agents");
            RemoveAssignmentsWhereNotAssignedToAgent(activeAssignments, activeAgents);

            // Re-balance if required if assignment counts are out of tolerance
            RebalanceIfRequired(activeTargets, activeAgents, activeAssignments);

            // Assign un-assigned targets to agents
            AssignTargetsToAgents(activeTargets, activeAgents, activeAssignments);

            // Update the DB with assignment delta
            await UpdateAssignments(activeAssignments, token);
        }

        private async Task<IReadOnlyList<AgentRecord>> GetActiveAgents(CancellationToken token) =>
            (await _agentContainer.ListAll(token))
                .Where(x => x.IsAgentRunning(_option.OfflineTolerance))
                .ToArray();

        private async Task<IReadOnlyList<TargetRecord>> GetActiveTargets(CancellationToken token) =>
            (await _targetContainer.ListAll(token))
                .Where(x => x.Enabled)
                .ToArray();

        private void RemoveAssignmentsWhereNotAssignedToAgent(IDictionary<string, AgentAssignmentRecord> activeAssignment, IReadOnlyList<AgentRecord> agentRecords) => activeAssignment.Values
               .Where(x => !agentRecords.Any(y => y.Id == x.AgentId))
               .ToArray()
               .ForEach(x => activeAssignment.Remove(x.Id));

        private int AgentAssignmentCount(AgentRecord agentRecord, IDictionary<string, AgentAssignmentRecord> activeAssignment) => activeAssignment.Values.Count(x => x.AgentId == agentRecord.Id);

        private void RebalanceIfRequired(IReadOnlyList<TargetRecord> activeTargets, IReadOnlyList<AgentRecord> activeAgents, IDictionary<string, AgentAssignmentRecord> activeAssignment)
        {
            _logger.LogTrace($"{nameof(RebalanceIfRequired)}: Check");

            int agentGoal = Math.Max(activeTargets.Count / activeAgents.Count, 1);

            bool overLimit = activeAgents
                .Select(x => AgentAssignmentCount(x, activeAssignment))
                .Any(x => agentGoal - x > (int)(agentGoal * .8));

            if (!overLimit) return;

            _logger.LogInformation($"{nameof(RebalanceIfRequired)}: Rebalancing assignments");
            activeAssignment.Clear();
        }

        private void AssignTargetsToAgents(IReadOnlyList<TargetRecord> activeTargets, IReadOnlyList<AgentRecord> activeAgents, IDictionary<string, AgentAssignmentRecord> activeAssignment)
        {
            _logger.LogTrace($"{nameof(AssignTargetsToAgents)}: Assign un-assigned targets to agents");

            var newTargets = activeTargets
                .Where(x => !activeAssignment.Values.Any(y => x.Id == y.TargetId))
                .ToArray();

            var newTargetsQueue = new Queue<TargetRecord>(newTargets);

            while (newTargetsQueue.TryDequeue(out TargetRecord targetRecord))
            {
                AgentRecord agent = activeAgents
                    .Select(x => (Agent: x, Count: AgentAssignmentCount(x, activeAssignment)))
                    .OrderBy(x => x.Count)
                    .Select(x => x.Agent)
                    .First();

                var newAssignment = new AgentAssignmentRecord(agent.Id, targetRecord.Id);

                if (!activeAssignment.ContainsKey(newAssignment.Id))
                {
                    activeAssignment.Add(newAssignment.Id, newAssignment);
                }
            }
        }

        private async Task UpdateAssignments(IDictionary<string, AgentAssignmentRecord> activeAssignments, CancellationToken token)
        {
            _logger.LogTrace($"{nameof(UpdateAssignments)}: Update assignments based on load balancing work");
            IReadOnlyList<AgentAssignmentRecord> currentList = await _agentAssignmentContainer.ListAll(token);

            // Delete assignments that are in current list but not in active assignments
            foreach (AgentAssignmentRecord record in currentList)
            {
                if (!activeAssignments.ContainsKey(record.Id))
                {
                    await _agentAssignmentContainer.Delete(record.Id);
                }
            }

            // Add assignments that are not in current list
            foreach (AgentAssignmentRecord record in activeAssignments.Values)
            {
                if (!currentList.Any(x => record.Id == x.Id))
                {
                    await _agentAssignmentContainer.Set(record, token: token);
                }
            }
        }
    }
}

