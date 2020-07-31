using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;
using WatcherSdk.Models;

namespace WatcherSdk.Records
{
    public class Record<T> : IRecord where T : class, IRecord
    {
        public Record(T value)
        {
            value.VerifyNotNull(nameof(value));

            Value = value;
        }

        public Record(T value, ETag eTag)
        {
            value.VerifyNotNull(nameof(value));
            eTag.VerifyNotNull(nameof(eTag));

            Value = value;
            ETag = eTag;
        }

        public T Value { get; }
        public ETag? ETag { get; }

        public string Id => Value.Id;

        public void Prepare() => Value.Prepare();
    }
}
