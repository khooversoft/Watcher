using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WatcherRepository.Models;

namespace WatcherRepository.Records
{
    public class TargetRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public string? Description { get; set; }

        public string? Url { get; set; }

        public IList<StatusCodeMap>? StatusCodeMaps { get; set; }

        public IList<BodyElementMap>? BodyElementMaps { get; set; }

        public string? TargetType { get; set; }

        public bool Enabled { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is TargetRecord record &&
                   Id == record.Id &&
                   Description == record.Description &&
                   Url == record.Url &&
                   Enumerable.SequenceEqual(StatusCodeMaps, record.StatusCodeMaps) &&
                   Enumerable.SequenceEqual(BodyElementMaps, record.BodyElementMaps) &&
                   TargetType == record.TargetType &&
                   Enabled == record.Enabled;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Description, Url, StatusCodeMaps, BodyElementMaps, TargetType, Enabled);

        public static bool operator ==(TargetRecord? left, TargetRecord? right) => EqualityComparer<TargetRecord>.Default.Equals(left!, right!);

        public static bool operator !=(TargetRecord? left, TargetRecord? right) => !(left == right);
    }
}
