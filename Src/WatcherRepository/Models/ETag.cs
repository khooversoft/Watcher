using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;

namespace WatcherRepository.Models
{
    public class ETag
    {
        public ETag(string eTag)
        {
            eTag.VerifyNotEmpty(nameof(eTag));

            Value = eTag;
        }

        public string Value { get; }

        public override bool Equals(object? obj)
        {
            return obj is ETag tag &&
                   Value == tag.Value;
        }

        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(ETag? left, ETag? right) => EqualityComparer<ETag>.Default.Equals(left!, right!);

        public static bool operator !=(ETag? left, ETag? right) => !(left == right);

        public static implicit operator string(ETag eTag) => eTag.Value;

        public static explicit operator ETag(string eTag) => new ETag(eTag);
    }
}
