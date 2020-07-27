using Toolbox.Services;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;

namespace WatcherCmd.Application
{
    internal interface IOption
    {
        bool Agent { get; }
        bool Balance { get; }
        bool Clear { get; }
        string? ConfigFile { get; }
        bool Create { get; }
        bool Delete { get; }
        bool Help { get; }
        KeyVaultOption? KeyVault { get; }
        bool List { get; }
        IPropertyResolver PropertyResolver { get; }
        ISecretFilter SecretFilter { get; }
        string? SecretId { get; }
        CosmosWatcherOption Store { get; }
        bool Target { get; }
    }
}