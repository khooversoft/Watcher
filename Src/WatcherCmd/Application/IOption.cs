using Toolbox.Services;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;

namespace WatcherCmd.Application
{
    internal interface IOption
    {
        bool Agent { get; }
        bool Assignment { get; }
        bool Balance { get; }
        bool Clear { get; }
        string? ConfigFile { get; }
        bool Create { get; }
        bool Delete { get; }
        string? File { get; }
        bool Help { get; }
        string? Id { get; }
        KeyVaultOption? KeyVault { get; }
        bool List { get; }
        string? LogFolder { get; }
        IPropertyResolver PropertyResolver { get; }
        ISecretFilter SecretFilter { get; }
        string? SecretId { get; }
        CosmosWatcherOption Store { get; }
        bool Target { get; }
        bool Template { get; }
    }
}