﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WatcherSdk.Records
{
    public interface IRecord : IRecordPrepare
    {
        string Id { get; }

        bool Equals(object? obj);
    }
}
