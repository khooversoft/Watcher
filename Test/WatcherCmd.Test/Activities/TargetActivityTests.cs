using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WatcherSdk.Models;
using WatcherSdk.Records;
using Xunit;

namespace WatcherCmd.Test.Activities
{
    public class TargetActivityTests : ActivityTestBase<TargetRecord>
    {
        public TargetActivityTests()
            : base("Target")
        {
        }

        [Fact]
        public async Task TestTargetFullLiveCycle_ShouldSucceeded() => await base.RunFullLifeCycleTests(() => new TargetRecord
        {
            Id = "Target_1",
            Description = "Target 1 record",
            Url = "http://localhost:5010",
            StatusCodeMaps = new[]
            {
                new StatusCodeMap { HttpStatusCode = HttpStatusCode.OK, State = TargetState.Ok }
            },
            TargetType = "rest",
            Enabled = true,
        });

        [Fact]
        public async Task RequestTemplateForEntityTest() => await base.RequestTemplateForEntity();

        [Fact]
        public async Task ClearCollectionTest() => await base.TestClearCollection(x => new TargetRecord
        {
            Id = $"Target_{x}",
            Description = $"Target {x} record",
            Url = $"http://localhost:501{x}",
            StatusCodeMaps = new[]
            {
                new StatusCodeMap { HttpStatusCode = HttpStatusCode.OK, State = TargetState.Ok }
            },
            TargetType = "rest",
            Enabled = true,
        }, 10);

    }
}
