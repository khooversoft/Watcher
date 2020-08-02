using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Services;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;

namespace WatcherAgent.Application
{
    internal class Option : IOption
    {
        public Option() { }

        public bool Help { get; set; }
        public string? ConfigFile { get; set; }
        public string? LogFolder { get; set; }

        public int Port { get; set; } = 5001;

        public string AgentId { get; set; } = null!;

        public string ServiceUri { get; set; } = null!;

        public string? SecretId { get; set; }

        public CosmosWatcherOption Store { get; set; } = null!;

        public KeyVaultOption? KeyVault { get; set; }

        public string GetServiceUrl() => Resolve(ServiceUri);

        public IReadOnlyList<KeyValuePair<string, string>> GetProperties() => new[]
        {
            new KeyValuePair<string, string>(nameof(Port), Port.ToString()),
            new KeyValuePair<string, string>(nameof(AgentId), AgentId),
        };

        public string Resolve(string value) => new PropertyResolver(GetProperties()).Resolve(value);
    }
}

