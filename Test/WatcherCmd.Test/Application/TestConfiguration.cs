using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using WatcherCmd.Application;
using Toolbox.Tools;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace WatcherCmd.Test.Application
{
    public class TestConfiguration
    {
        public const string TestConfigResourceId = "WatcherCmd.Test.Application.TestConfig.json";

        public TestConfiguration() { }

        public string[] BuildArgs(params string[] args) => TestConfigResourceId.GetOptionArguments(args);

        internal IOption GetOption(params string[] args) => new OptionBuilder()
            .SetArgs(TestConfigResourceId.GetOptionArguments(args))
            .Build();
    }
}
