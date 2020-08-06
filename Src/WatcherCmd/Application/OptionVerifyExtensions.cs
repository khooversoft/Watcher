using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;
using Toolbox.Extensions;
using WatcherCmd.Application;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;

namespace WatcherCmd.Application
{
    internal static class OptionVerifyExtensions
    {
        private static bool TestStore(Option option)
        {
            option.Store.VerifyNotNull("Store options are not specified");
            option.Store?.ConnectionString.VerifyNotEmpty($"{nameof(option.Store)}:{nameof(option.Store.ConnectionString)} is required");
            option.Store?.AccountKey.VerifyNotEmpty($"{nameof(option.Store)}:{nameof(option.Store.AccountKey)} is required");
            option.Store?.DatabaseName.VerifyNotEmpty($"{nameof(option.Store)}:{nameof(option.Store.DatabaseName)} is required");

            return true;
        }

        private static Func<Option, bool>[] _tests = new Func<Option, bool>[]
        {
            x => x.Help == true,

            x => x.Balance == true
                && TestStore(x),

            x => x.Get
                && TestStore(x)
                && (x.Agent || x.Target).VerifyAssert(x => x == true, "Get requires either 'Target' or 'Agent'")
                && (x.Id).VerifyNotEmpty("Get requires 'Id'") != null
                && x.File.VerifyNotEmpty("Get requires 'File'") != null,

            x => x.List
                && TestStore(x)
                && (x.Agent || x.Target).VerifyAssert(x => x == true, "List requires either 'Target' or 'Agent'"),

            x => x.Delete
                && TestStore(x)
                && (x.Agent || x.Target).VerifyAssert(x => x == true, "Delete requires either 'Target' or 'Agent'")
                && (x.Id).VerifyNotEmpty("Get requires 'Id'") != null,

            x => x.Template
                && (x.Agent || x.Target).VerifyAssert(x => x == true, "Template requires either 'Target' or 'Agent'")
                && x.File.VerifyNotEmpty("Template requires 'File'") != null,

            x => x.Import
                && TestStore(x)
                && x.File.VerifyNotEmpty("Import requires 'File'") != null,

            x => x.Monitor
                && TestStore(x),
        };

        public static Option Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));

            _tests
                .TakeWhile(x => !x(option))
                .Count()
                .VerifyAssert(x => x != _tests.Length, "Unknown command");

            return option;
        }
    }
}
