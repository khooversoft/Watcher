using System.Threading.Tasks;
using WatcherSdk.Records;
using Xunit;

namespace WatcherCmd.Test.Activities
{
    public class AgentAssignmentTests : ActivityTestBase<AgentAssignmentRecord>
    {
        public AgentAssignmentTests()
            : base("Assignment")
        {
        }

        [Fact]
        public async Task TestAgentAssignmentFullLiveCycle_ShouldSucceeded() => await base.RunFullLifeCycleTests(() => new AgentAssignmentRecord("agent_1", "target_1"));

        [Fact]
        public async Task RequestTemplateForEntityTest() => await base.RequestTemplateForEntity();


        [Fact]
        public async Task ClearCollectionTest() => await base.TestClearCollection(x => new AgentAssignmentRecord($"agent_{x}", $"target_{x}"), 10);
    }
}
