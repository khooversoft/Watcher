using System.Collections.Generic;
using Toolbox.Services;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;

namespace WatcherCmd.Application
{
    internal class Option : IOption
    {
        public bool Help { get; set; }

        public string? ConfigFile { get; set; }

        public bool Agent { get; set; }
        public bool Target { get; set; }
        public bool Balance { get; set; }

        public bool Create { get; set; }
        public bool List { get; set; }
        public bool Delete { get; set; }
        public bool Clear { get; set; }

        public string? SecretId { get; set; }

        public CosmosWatcherOption Store { get; set; } = null!;

        public KeyVaultOption? KeyVault { get; set; }

        public IPropertyResolver PropertyResolver { get; set; } = null!;

        public ISecretFilter SecretFilter { get; set; } = null!;
    }
}
