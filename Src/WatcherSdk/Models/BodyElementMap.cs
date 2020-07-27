using System;
using System.Collections.Generic;
using System.Text;

namespace WatcherSdk.Models
{
    public class BodyElementMap
    {
        public string? Path { get; set; }

        public string? CompareTo { get; set; }

        public TargetState State { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is BodyElementMap map &&
                   Path == map.Path &&
                   CompareTo == map.CompareTo &&
                   State == map.State;
        }

        public override int GetHashCode() => HashCode.Combine(Path, CompareTo, State);

        public static bool operator ==(BodyElementMap? left, BodyElementMap? right) => EqualityComparer<BodyElementMap>.Default.Equals(left!, right!);

        public static bool operator !=(BodyElementMap? left, BodyElementMap? right) => !(left == right);
    }
}
