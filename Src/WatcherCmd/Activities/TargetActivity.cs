using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Services;
using WatcherCmd.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherCmd.Activities
{
    internal class TargetActivity : ActivityEntityBase<TargetRecord>
    {
        public TargetActivity(IOption option, IRecordContainer<TargetRecord> recordContainer, IJson json, ILogger<TargetActivity> logger)
            : base(option, recordContainer, json, logger, "Target")
        {
        }
    }
}
