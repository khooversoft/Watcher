using System;
using System.Collections.Generic;
using System.Text;

namespace WatcherSdk.Records
{
    public class RecordBase
    {
        public RecordBase() { RecordType = null!; }

        protected RecordBase(string recordType) => RecordType = recordType;

        public string RecordType { get; set; }
    }
}
