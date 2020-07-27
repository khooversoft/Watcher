using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;
using Toolbox.Extensions;
using WatcherCmd.Application;

namespace WatcherCmd.Application
{
    internal static class OptionExtensions
    {
        private static readonly IReadOnlyList<Func<Option, bool>> _scenarios = new List<Func<Option, bool>>
        {
            // Scenarios are listed in priority order
            option => new[] { option.Create, option.List, option.Delete, option.Clear }
                    .Where(x => x == true)
                    .Count()
                    .Action(x => x.VerifyAssert(y => option.Balance || y == 1, "Must specify only one command, Create, List, Delete, Clear"))
                    == 1,

            option => new[] { option.Agent, option.Target, option.Balance }
                    .Where(x => x == true)
                    .Count()
                    .Action(x => x.VerifyAssert(y => y == 1, "Must specify only one entity, Agent, Target, or command Balance"))
                    == 1,
        };

        public static Option Verify(this Option option)
        {
            _scenarios
                .Select(x => x(option) ? 1 : 0)
                .Sum()
                .VerifyAssert(x => x != 0, $"Unknown command(s).  Use 'help' to list valid commands");

            return option;
        }

        public static Option Bind(this IConfiguration configuration)
        {
            configuration.VerifyNotNull(nameof(configuration));

            var option = new Option();
            configuration.Bind(option, x => x.BindNonPublicProperties = true);
            return option;
        }
    }
}
