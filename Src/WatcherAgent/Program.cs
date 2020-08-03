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
using Watcher.Cosmos.Repository.Application;

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

            CreateHostBuilder(args, option, new BoundedQueue<string>(1000))
                .Build()
                .Run();
        }

        internal static IHostBuilder CreateHostBuilder(string[] args, IOption option, BoundedQueue<string> queue) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureServices(service =>
                 {
                     service.AddSingleton<IOption>(option);
                     service.AddSingleton<ICosmosWatcherOption>(option.Store);
                     service.AddSingleton<BoundedQueue<string>>(queue);
                 })
                .ConfigureLogging(builder =>
                {
                    builder.AddMemoryLogger(queue);

                    if (!option.LogFolder?.IsEmpty() == true) builder.AddFileLogger(option.LogFolder!, "WatcherAgent");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
