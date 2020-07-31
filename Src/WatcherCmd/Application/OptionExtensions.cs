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
    internal static class OptionExtensions
    {
        private static void IsCommand(Option option) => new[] { option.Agent, option.Target, option.Assignment, option.Balance }
                .Where(x => x == true)
                .Count()
                .VerifyAssert(x => x == 1, "Must specify one of the following commands: Agent, Target, Assignment, Balance, or Help");

        private static void IsOperation(Option option) => new[] { option.Create, option.List, option.Delete, option.Clear, option.Template }
                .Where(x => x == true)
                .Count()
                .VerifyAssert(x => x == 1, "Must specify an action: Create, List, Delete, Clear, or Template");

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

            IsCommand(option);
            TestStore(option);

            if (option.Create || option.Template || option.Assignment)
            {
                IsOperation(option);
            }

            if (option.Create || option.Template)
            {
                if (option.File.IsEmpty()) throw new ArgumentException("Create or Template requires a 'File={file}' to input or create template for entity");
                if (!File.Exists(option.File)) throw new ArgumentException($"File {option.File} does not exist");
            }

            if (option.Delete)
            {
                if (option.Id.IsEmpty()) throw new ArgumentException("Delete requires a 'Id={id}' for the entity record id");
            }

            return option;
        }

        public static Option Bind(this IConfiguration configuration)
        {
            configuration.VerifyNotNull(nameof(configuration));

            //var option = new Option();
            var option = new Option();
            configuration.Bind(option, x => x.BindNonPublicProperties = true);
            return option;
        }
   }
}
