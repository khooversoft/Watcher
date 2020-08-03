using System;
using System.Collections.Generic;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;

namespace WatcherAgent.Application
{
    internal interface IOption
    {
        string AgentId { get; set; }
        string? ConfigFile { get; set; }
        bool Help { get; set; }
        KeyVaultOption? KeyVault { get; set; }
        string? LogFolder { get; set; }
        int Port { get; set; }
        string? SecretId { get; set; }
        string ServiceUri { get; set; }
        CosmosWatcherOption Store { get; set; }
        TimeSpan StoreSyncFrequency { get; set; }

        IReadOnlyList<KeyValuePair<string, string>> GetProperties();
        string GetServiceUrl();
        string Resolve(string value);
    }
}