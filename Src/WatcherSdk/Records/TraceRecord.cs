using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Toolbox.Tools;
using WatcherSdk.Models;

namespace WatcherSdk.Records
{
    public class TraceRecord : IRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? TargetId { get; set; }

        public string? AgentId { get; set; }

        public string? Url { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string? Body { get; set; }

        public TargetState TargetState { get; set; }

        public string? Exception { get; set; }

        public long? ProbeMs { get; set; }

        public void Prepare() => Id = Id.VerifyNotEmpty(nameof(Id)).ToLowerInvariant();

        public override string ToString()
        {
            var lines = new[]
            {
                $"Id={Id}",
                $"TargetId={TargetId}",
                $"AgentId={AgentId}",
                $"Url={Url}",
                $"HttpStatusCode={HttpStatusCode}",
                $"Body={Body}",
                $"TargetState={TargetState}",
                $"Exception={Exception}",
                $"ProbeMs={ProbeMs}",
            };

            return string.Join(", ", lines);
        }


        public override bool Equals(object? obj)
        {
            return obj is TraceRecord record &&
                   Id.ToLowerInvariant() == record.Id.ToLowerInvariant() &&
                   Timestamp == record.Timestamp &&
                   TargetId == record.TargetId &&
                   AgentId == record.AgentId &&
                   Url == record.Url &&
                   HttpStatusCode == record.HttpStatusCode &&
                   Body == record.Body &&
                   TargetState == record.TargetState &&
                   Exception == record.Exception &&
                   ProbeMs == record.ProbeMs;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Timestamp, TargetId, AgentId);

        public static bool operator ==(TraceRecord? left, TraceRecord? right) => EqualityComparer<TraceRecord>.Default.Equals(left!, right!);

        public static bool operator !=(TraceRecord? left, TraceRecord? right) => !(left == right);
    }
}
