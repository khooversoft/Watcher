using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Services;
using Toolbox.Tools;

namespace Watcher.Cosmos.Repository.Application
{
    public class CosmosWatcherOption : ICosmosWatcherOption
    {
        /// <summary>
        /// Connection string for Cosmos DB account.  The string should use placeholders for the account key.
        /// 
        /// Example: "AccountEndpoint=https://test-db.documents.azure.com:443/;AccountKey={AccountKey};",
        /// </summary>
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Account key secret, should be loaded from a secret file, key vault or specified outside source code paths.
        /// </summary>
        public string AccountKey { get; set; } = null!;

        /// <summary>
        /// Trace collection Time to Live default value.  Records older then this will be removed by Cosmos
        /// </summary>
        public TimeSpan TraceTraceTTL { get; set; } = TimeSpan.FromDays(30);

        /// <summary>
        /// Database name
        /// </summary>
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// Agent off-line tolerance time period.  Any agent that has not recorded a heart beat within
        /// this tolerance will be considered off-line and not available for load balancing.
        /// </summary>
        public TimeSpan OfflineTolerance { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Frequency of the agent's heart beat
        /// </summary>
        public TimeSpan HeartbeatFrequency { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Verify required values are present
        /// </summary>
        public void Verify()
        {
            ConnectionString.VerifyNotEmpty(nameof(ConnectionString));
            AccountKey.VerifyNotEmpty(nameof(AccountKey));
            DatabaseName.VerifyNotEmpty(nameof(DatabaseName));
        }

        /// <summary>
        /// Get resolved connection string, account key is resolved in the connection string
        /// </summary>
        /// <returns></returns>
        public string GetResolvedConnectionString()
        {
            Verify();

            return Resolve(ConnectionString);
        }

        /// <summary>
        /// Get property values for resolving properties
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<KeyValuePair<string, string>> GetProperties() => new[]
        {
            new KeyValuePair<string, string>(nameof(AccountKey), AccountKey),
            new KeyValuePair<string, string>(nameof(DatabaseName), DatabaseName),
        };

        /// <summary>
        /// Resolve string with option's property values
        /// </summary>
        /// <param name="value">subject to resolve</param>
        /// <returns></returns>
        public string Resolve(string value) => new PropertyResolver(GetProperties()).Resolve(value);
    }
}
