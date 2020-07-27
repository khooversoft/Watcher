using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WatcherSdk.Models
{
    public class StatusCodeMap
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public TargetState State { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is StatusCodeMap map &&
                   HttpStatusCode == map.HttpStatusCode &&
                   State == map.State;
        }

        public override int GetHashCode() => HashCode.Combine(HttpStatusCode, State);

        public static bool operator ==(StatusCodeMap? left, StatusCodeMap? right) => EqualityComparer<StatusCodeMap>.Default.Equals(left!, right!);

        public static bool operator !=(StatusCodeMap? left, StatusCodeMap? right) => !(left == right);
    }
}
