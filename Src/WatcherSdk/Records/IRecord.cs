using System;
using System.Collections.Generic;
using System.Text;

namespace WatcherSdk.Records
{
    public interface IRecord
    {
        string Id { get; }

        void Prepare();
    }
}
