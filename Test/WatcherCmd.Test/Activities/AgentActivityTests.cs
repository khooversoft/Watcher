using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Services;
using Toolbox.Tools;
using Watcher.Cosmos.Repository;
using WatcherCmd.Application;
using WatcherCmd.Test.Application;
using WatcherSdk.Models;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Xunit;

namespace WatcherCmd.Test.Activities
{
    public class AgentActivityTests : ActivityTestBase<AgentRecord>
    {
        public AgentActivityTests()
            : base("Agent")
        {
        }

        [Fact]
        public async Task TestAgentFullLiveCycle_ShouldSucceeded() => await base.RunFullLifeCycleTests(() => new AgentRecord
        {
            Id = "agent_1",
            State = AgentState.Running,
        });

        [Fact]
        public async Task RequestTemplateForEntityTest() => await base.RequestTemplateForEntity();

        [Fact]
        public async Task ClearCollectionTest() => await base.TestClearCollection(x => new AgentRecord
        {
            Id = $"agent_{x}",
            State = AgentState.Running,
        }, 10);
    }
}
