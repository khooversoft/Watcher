using Toolbox.Services;
using Watcher.Cosmos.Repository.Application;
using WatcherSdk.Models;

namespace WatcherCmd.Application
{
    internal interface IOption
    {
        bool Agent { get; set; }
        bool Assignment { get; set; }
        bool Balance { get; set; }
        bool Clear { get; set; }
        string? ConfigFile { get; set; }
        bool Create { get; set; }
        bool Delete { get; set; }
        string? File { get; set; }
        bool Get { get; set; }
        bool Help { get; set; }
        string? Id { get; set; }
        KeyVaultOption? KeyVault { get; set; }
        bool List { get; set; }
        string? LogFolder { get; set; }
        IPropertyResolver PropertyResolver { get; set; }
        ISecretFilter SecretFilter { get; set; }
        string? SecretId { get; set; }
        CosmosWatcherOption Store { get; set; }
        bool Target { get; set; }
        bool Template { get; set; }
    }
}