using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WatcherRepository.Models;

namespace WatcherRepository.Records
{
    public class TraceRecord
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

        public override bool Equals(object? obj)
        {
            return obj is TraceRecord record &&
                   Id == record.Id &&
                   Timestamp == record.Timestamp &&
                   TargetId == record.TargetId &&
                   AgentId == record.AgentId &&
                   Url == record.Url &&
                   HttpStatusCode == record.HttpStatusCode &&
                   Body == record.Body &&
                   TargetState == record.TargetState;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Timestamp, TargetId, AgentId, Url, HttpStatusCode, Body, TargetState);

        public static bool operator ==(TraceRecord? left, TraceRecord? right) => EqualityComparer<TraceRecord>.Default.Equals(left!, right!);

        public static bool operator !=(TraceRecord? left, TraceRecord? right) => !(left == right);
    }
}
