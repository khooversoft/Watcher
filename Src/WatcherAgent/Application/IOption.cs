using Toolbox.Services;
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
        string? SecretId { get; set; }
        CosmosWatcherOption Store { get; set; }
        int Port { get; }
    }
}