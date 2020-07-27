using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;

namespace WatcherCmd.Application
{
    internal static class OptionHelpExtensions
    {
        public static IReadOnlyList<string> GetHelp(this IOption _)
        {
            return new[]
            {
                "ML Host command line interface commands",
                "",
                "Help                  : Display help",
                "List                  : List active models",
                "ConfigFile            : Load JSON configuration file",
                "",
                "To work with Agent or Target records, specify the entity and operations." +
                "Example: ",
                "",
                "",
                "",
                "Entity",
                "  Agent               : Operate on Agent records",
                "  Target              : Operate on Target records",
                "",
                "Record operation records",
                "  Create              : Create agent or target record",
                "  List                : List agent or target records",
                "  Delete              : Delete agent or target record",
                "  Clear               : Delete all agent or target records",
                "",
                "",
                "Configuration for Watcher Repository (Cosmos DB)",
                "",
                "  SecretId={secretId}                       : Use .NET Core configuration secret json file.  SecretId indicates which secret file to use.",
                "",
                "  Store:ConnectionString={data}             : Cosmo DB account connection string",
                "  Store:AccountKey={accountKey}             : Account key for Cosmos DB account",
                "  Store:AccountName={accountName}           : Azure Blob Storage account name (required)",
                "  Store:TraceTraceTTL={n seconds}           : Trace collection TTL setting, used when creating the Trace collection.",
                "  Store:DatabaseName={data}                 : Cosmos database name",
                "  Store:OfflineTolerance={data}             : Agent off-line tolerance time period. (TimeSpan format 'hh:mm:ss')",
                "  Store:HeartbeatFrequency={data}           : Agent's heartbeat frequency time period. (TimeSpan format 'hh:mm:ss')",
                "",
                "  If 'BlobStore:AccountKey' is not specified then key vault will be used to retrieve the account key.",
                "    KeyVault:KeyVaultName={keyVaultName}    : Name of the Azure key vault (required if 'Store:AccountKey' is not specified",
                "    KeyVault:KeyName={keyName}              : Name of the Azure key vault's key where the 'Store:AcountKey' is stored",
            };
        }

        public static void DumpConfigurations(this IOption option)
        {
            const int maxWidth = 80;

            option.GetConfigValues()
                .Select(x => "  " + x)
                .Prepend(new string('=', maxWidth))
                .Prepend("Current configurations")
                .Prepend(string.Empty)
                .Append(string.Empty)
                .Append(string.Empty)
                .ForEach(x => Console.WriteLine(option.SecretFilter?.FilterSecrets(x) ?? x));
        }
    }
}
