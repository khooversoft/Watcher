using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WatcherRepository.Records
{
    public class AgentAssignmentRecord
    {
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

        public static bool operator ==(AgentAssignmentRecord? left, AgentAssignmentRecord? right) => EqualityComparer<AgentAssignmentRecord>.Default.Equals(left!, right!);

        public static bool operator !=(AgentAssignmentRecord? left, AgentAssignmentRecord? right) => !(left == right);
    }
}
