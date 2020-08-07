using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Toolbox.Extensions;
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

        public string? TargetState { get; set; }

        public string? Exception { get; set; }

        public long? ProbeMs { get; set; }

        public void Prepare() => Id = Id.VerifyNotEmpty(nameof(Id)).ToLowerInvariant();

        public override string ToString() => GetPropertyValues()
            .Select(x => $"{x.Key}={x.Value}")
            .Func(x => string.Join(", ", x));

        public IReadOnlyList<KeyValuePair<string, string?>> GetPropertyValues() => new KeyValuePair<string, string?>[]
        {
            new KeyValuePair<string, string?>("Id", Id),
            new KeyValuePair<string, string?>("TargetId", TargetId),
            new KeyValuePair<string, string?>("AgentId", AgentId),
            new KeyValuePair<string, string?>("Url", Url),
            new KeyValuePair<string, string?>("HttpStatusCode", HttpStatusCode.ToString()),
            new KeyValuePair<string, string?>("Body", Body),
            new KeyValuePair<string, string?>("TargetState", TargetState),
            new KeyValuePair<string, string?>("Exception", Exception),
            new KeyValuePair<string, string?>("ProbeMs", ProbeMs?.ToString() ?? string.Empty),
        };

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
