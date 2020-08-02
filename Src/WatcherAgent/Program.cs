using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatcherAgent.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Logging;

namespace WatcherAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IOption option = new OptionBuilder()
                .SetArgs(args)
                .SetConfigFile("appsettings.json")
                .Build();

            CreateHostBuilder(args, option)
                .Build()
                .Run();
        }

        internal static IHostBuilder CreateHostBuilder(string[] args, IOption option) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureServices(service =>
                 {
                     service.AddSingleton<IOption>(option);
                 })
                .ConfigureLogging(builder =>
                {
                    if (!option.LogFolder?.IsEmpty() == true) builder.AddLogFile(option.LogFolder!, "WatcherAgent");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
