using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Toolbox.Tools;
using Watcher.Cosmos.Repository.Application;

namespace Cosmos.Watcher.Repository.Test.Application
{
    public class TestOptionBuilder
    {
        private const string _jsonResourceId = "WatcherRepository.Test.Application.DevConfig.json";

        public ICosmosWatcherOption Build(params string[] args)
        {
            using Stream stream = Assembly
                .GetAssembly(typeof(TestOptionBuilder))!
                .GetManifestResourceStream(_jsonResourceId)
                .VerifyNotNull($"Resource id {_jsonResourceId} not found.");

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .AddUserSecrets("WatcherRepository.Test")
                .AddCommandLine(args)
                .Build();

            CosmosWatcherOption option = new CosmosWatcherOption();
            config.Bind(option, x => x.BindNonPublicProperties = true);
            option.Verify();

            return option;
        }
    }
}
