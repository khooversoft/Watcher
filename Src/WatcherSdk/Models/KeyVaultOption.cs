using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;

namespace WatcherSdk.Models
{
    public class KeyVaultOption
    {
        public string? KeyVaultName { get; set; }

        public string? KeyName { get; set; }
    }

    public static class OptionExtensions
    {
        public static void Verify(this KeyVaultOption keyVaultOption)
        {
            keyVaultOption.VerifyNotNull("KeyVault option is required");
            keyVaultOption.KeyVaultName.VerifyNotEmpty($"{nameof(keyVaultOption.KeyVaultName)} is missing");
            keyVaultOption.KeyName.VerifyNotEmpty($"{nameof(keyVaultOption.KeyName)} is missing");
        }
    }
}
