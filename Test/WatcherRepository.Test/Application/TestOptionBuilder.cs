using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Toolbox.Tools;

namespace WatcherRepository.Test.Application
{
    public class TestOptionBuilder
    {
        private const string _jsonResourceId = "WatcherRepository.Test.Application.DevConfig.json";

        public IWatcherOption Build()
        {
            using Stream stream = Assembly
                .GetAssembly(typeof(TestOptionBuilder))!
                .GetManifestResourceStream(_jsonResourceId)
                .VerifyNotNull($"Resource id {_jsonResourceId} not found.");

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .AddUserSecrets("WatcherRepository.Test")
                .Build();

            WatcherOption option = new WatcherOption();
            config.Bind(option, x => x.BindNonPublicProperties = true);
            option.Verify();

            return option;
        }
    }
}
