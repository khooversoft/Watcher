using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace WatcherRepository.Records
{
    public class AgentAssignmentRecord : IRecord
    {
        public AgentAssignmentRecord() { }

        public AgentAssignmentRecord(string agentId, string targetId)
        {
            agentId = agentId.VerifyNotEmpty(nameof(agentId)).ToLowerInvariant();
            targetId =  targetId.VerifyNotEmpty(nameof(targetId)).ToLowerInvariant();

            Id = CreateId(agentId, targetId);
            AgentId = agentId;
            TargetId = targetId;
        }

        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public string AgentId { get; set; } = null!;

        public string? TargetId { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is AgentAssignmentRecord record &&
                   Id == record.Id &&
                   AgentId == record.AgentId &&
                   TargetId == record.TargetId;
        }

        public override int GetHashCode() => HashCode.Combine(Id, AgentId, TargetId);

        public void Prepare()
        {
            AgentId.VerifyNotEmpty(nameof(AgentId));
            TargetId.VerifyNotEmpty(nameof(TargetId));

            if (Id.IsEmpty()) Id = CreateId(AgentId, TargetId);
            Id = Id.ToLowerInvariant();
        }

        public static bool operator ==(AgentAssignmentRecord? left, AgentAssignmentRecord? right) => EqualityComparer<AgentAssignmentRecord>.Default.Equals(left!, right!);

        public static bool operator !=(AgentAssignmentRecord? left, AgentAssignmentRecord? right) => !(left == right);

        private static string CreateId(string agentId, string targetId) => $"{agentId}.{targetId}";
    }
}
