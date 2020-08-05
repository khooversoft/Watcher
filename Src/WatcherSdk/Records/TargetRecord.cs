using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;
using WatcherSdk.Models;

namespace WatcherSdk.Records
{
    public class TargetRecord : RecordBase, IRecord
    {
        public TargetRecord() : base(nameof(TargetRecord)) { }

        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public string? Description { get; set; }

        /// <summary>
        /// Required URL to see if the service is ready for operations
        /// </summary>
        public string? ReadyUrl { get; set; }

        /// <summary>
        /// Optional URL to see if the service is running, "ready" returns 503
        /// </summary>
        public string? RunningUrl { get; set; }

        public IList<StatusCodeMap>? StatusCodeMaps { get; set; }

        public IList<BodyElementMap>? BodyElementMaps { get; set; }

        public string? TargetType { get; set; }

        public bool Enabled { get; set; }

        public int? FrequencyInSeconds { get; set; } = (int)TimeSpan.FromMinutes(5).TotalSeconds;

        public string? AssignedAgentId { get; set; }

        public override string ToString() => $"Id={Id}, TargetType={TargetType}, ReadyUrl={ReadyUrl}, RunningUrl={RunningUrl}, AssignedAgentId={AssignedAgentId}";

        public override bool Equals(object? obj)
        {
            return obj is TargetRecord record &&
                   Id.ToLowerInvariant() == record.Id.ToLowerInvariant() &&
                   Description == record.Description &&
                   ReadyUrl == record.ReadyUrl &&
                   RunningUrl == record.RunningUrl &&
                   SequenceEqual(StatusCodeMaps, record.StatusCodeMaps) &&
                   SequenceEqual(BodyElementMaps, record.BodyElementMaps) &&
                   TargetType == record.TargetType &&
                   Enabled == record.Enabled &&
                   FrequencyInSeconds == record.FrequencyInSeconds &&
                   AssignedAgentId == record.AssignedAgentId;
        }

        public override int GetHashCode() => HashCode.Combine(Id);

        public void Prepare()
        {
            Id.VerifyNotEmpty(nameof(Id));
            ReadyUrl.VerifyNotEmpty(nameof(ReadyUrl));

            Id = Id.ToLowerInvariant();

            AssignedAgentId = AssignedAgentId
                .ToNullIfEmpty()
                ?.ToLowerInvariant();
        }

        public static bool operator ==(TargetRecord? left, TargetRecord? right) => EqualityComparer<TargetRecord>.Default.Equals(left!, right!);

        public static bool operator !=(TargetRecord? left, TargetRecord? right) => !(left == right);

        private static bool SequenceEqual<T>(IEnumerable<T>? source, IEnumerable<T>? value)
        {
            if (source == null || value == null) return source == value;
            return source.SequenceEqual(value);
        }
    }
}
