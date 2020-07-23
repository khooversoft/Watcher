using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;
using WatcherRepository.Models;

namespace WatcherRepository.Records
{
    public class AgentRecord : IRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public AgentState State { get; set; }

        public DateTime UtcHeartbeat { get; set; } = DateTime.UtcNow;

        public override bool Equals(object? obj)
        {
            return obj is AgentRecord record &&
                   Id.ToLowerInvariant() == record.Id.ToLowerInvariant() &&
                   State == record.State &&
                   UtcHeartbeat == record.UtcHeartbeat;
        }

        public override int GetHashCode() => HashCode.Combine(Id, State, UtcHeartbeat);

        public void Prepare() => Id = Id.VerifyNotEmpty(nameof(Id)).ToLowerInvariant();

        public static bool operator ==(AgentRecord? left, AgentRecord? right) => EqualityComparer<AgentRecord>.Default.Equals(left!, right!);

        public static bool operator !=(AgentRecord? left, AgentRecord? right) => !(left == right);
    }
}
