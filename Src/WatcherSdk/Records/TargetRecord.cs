using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;
using WatcherSdk.Models;

namespace WatcherSdk.Records
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

        public int? FrequencyInSeconds { get; set; } = (int)TimeSpan.FromMinutes(5).TotalSeconds;

        public override string ToString() => $"Id={Id}, TargetType={TargetType}, Url={Url}";

        public override bool Equals(object? obj)
        {
            return obj is TargetRecord record &&
                   Id.ToLowerInvariant() == record.Id.ToLowerInvariant() &&
                   Description == record.Description &&
                   Url == record.Url &&
                   SequenceEqual(StatusCodeMaps, record.StatusCodeMaps) &&
                   SequenceEqual(BodyElementMaps, record.BodyElementMaps) &&
                   TargetType == record.TargetType &&
                   Enabled == record.Enabled &&
                   FrequencyInSeconds == record.FrequencyInSeconds;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Description, Url, StatusCodeMaps, BodyElementMaps, TargetType, Enabled);

        public void Prepare() => Id = Id.VerifyNotEmpty(nameof(Id)).ToLowerInvariant();

        public static bool operator ==(TargetRecord? left, TargetRecord? right) => EqualityComparer<TargetRecord>.Default.Equals(left!, right!);

        public static bool operator !=(TargetRecord? left, TargetRecord? right) => !(left == right);

        private static bool SequenceEqual<T>(IEnumerable<T>? source, IEnumerable<T>? value)
        {
            if (source == null || value == null) return source == value;
            return source.SequenceEqual(value);
        }
    }
}
