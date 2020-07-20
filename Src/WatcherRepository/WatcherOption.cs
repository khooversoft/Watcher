using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Services;
using Toolbox.Tools;

namespace WatcherRepository
{
    public class WatcherOption : IWatcherOption
    {
        public string ConnectionString { get; set; } = null!;

        public string AccountKey { get; set; } = null!;

        public TimeSpan TraceTTL { get; set; } = TimeSpan.FromDays(30);

        public string Environment { get; set; } = null!;

        public string Tenant { get; set; } = null!;

        public void Verify()
        {
            ConnectionString.VerifyNotEmpty(nameof(ConnectionString));
            AccountKey.VerifyNotEmpty(nameof(AccountKey));
            Environment.VerifyNotEmpty(nameof(Environment));
            Tenant.VerifyNotEmpty(nameof(Tenant));
        }

        public string GetResolvedConnectionString()
        {
            Verify();

            var properties = new[]
            {
                new KeyValuePair<string, string>(nameof(AccountKey), AccountKey),
            };

            return new PropertyResolver(properties).Resolve(ConnectionString)!;
        }

        public string CreateContainerName(string subject)
        {
            Verify();
            return $"{Environment}.{Tenant}.{subject}";
        }
    }
}
