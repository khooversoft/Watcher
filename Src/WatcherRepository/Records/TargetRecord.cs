using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;
using WatcherRepository.Models;

namespace WatcherRepository.Records
{
    public class TargetRecord : IRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public string? Description { get; set; }

        public string? Url { get; set; }

        public IList<StatusCodeMap>? StatusCodeMaps { get; set; }

        public IList<BodyElementMap>? BodyElementMaps { get; set; }

        public string? TargetType { get; set; }

        public bool Enabled { get; set; }

        public TimeSpan? Frequency { get; set; } = TimeSpan.FromMinutes(5);

        public override bool Equals(object? obj)
        {
            return obj is TargetRecord record &&
                   Id.ToLowerInvariant() == record.Id.ToLowerInvariant() &&
                   Description == record.Description &&
                   Url == record.Url &&
                   Enumerable.SequenceEqual(StatusCodeMaps, record.StatusCodeMaps) &&
                   Enumerable.SequenceEqual(BodyElementMaps, record.BodyElementMaps) &&
                   TargetType == record.TargetType &&
                   Enabled == record.Enabled &&
                   Frequency == record.Frequency;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Description, Url, StatusCodeMaps, BodyElementMaps, TargetType, Enabled);

        public void Prepare() => Id = Id.VerifyNotEmpty(nameof(Id)).ToLowerInvariant();

        public static bool operator ==(TargetRecord? left, TargetRecord? right) => EqualityComparer<TargetRecord>.Default.Equals(left!, right!);

        public static bool operator !=(TargetRecord? left, TargetRecord? right) => !(left == right);
    }
}
