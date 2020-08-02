using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;
using Toolbox.Extensions;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using WatcherAgent.Application;

namespace WatcherAgent.Application
{
    internal static class OptionExtensions
    {
        private static void TestStore(Option option)
        {
            option.Store.VerifyNotNull("Store options are not specified");
            option.Store?.ConnectionString.VerifyNotEmpty($"{nameof(option.Store)}:{nameof(option.Store.ConnectionString)} is required");
            option.Store?.AccountKey.VerifyNotEmpty($"{nameof(option.Store)}:{nameof(option.Store.AccountKey)} is required");
            option.Store?.DatabaseName.VerifyNotEmpty($"{nameof(option.Store)}:{nameof(option.Store.DatabaseName)} is required");
        }

        public static Option Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));

            option.AgentId.VerifyNotEmpty($"{nameof(option.AgentId)} is required");
            option.ServiceUri.VerifyNotEmpty($"{nameof(option.ServiceUri)} Is required");
            TestStore(option);

            return option;
        }
    }
}
