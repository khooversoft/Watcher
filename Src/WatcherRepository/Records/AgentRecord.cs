using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WatcherRepository.Models;

namespace WatcherRepository.Records
{
    public class AgentRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public AgentState State { get; set; }

        public DateTime UtcHeartbeat { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is AgentRecord record &&
                   Id == record.Id &&
                   State == record.State &&
                   UtcHeartbeat == record.UtcHeartbeat;
        }

        public override int GetHashCode() => HashCode.Combine(Id, State, UtcHeartbeat);

        public static bool operator ==(AgentRecord? left, AgentRecord? right) => EqualityComparer<AgentRecord>.Default.Equals(left!, right!);

        public static bool operator !=(AgentRecord? left, AgentRecord? right) => !(left == right);
    }
}
