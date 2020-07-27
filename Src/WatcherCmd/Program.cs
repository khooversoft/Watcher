using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WatcherCmd.Application;
using Toolbox.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WatcherCmd.Activities;
using WatcherSdk.Repository;
using Watcher.Cosmos.Repository.Application;
using Watcher.Cosmos.Repository;

namespace WatcherCmd
{
    class Program
    {
        private const int _ok = 0;
        private const int _error = 1;
        private readonly string _programTitle = $"Watcher CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}";

        static async Task<int> Main(string[] args)
        {
            try
            {
                return await new Program().Run(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                DisplayStartDetails(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhanded exception: " + ex.ToString());
                DisplayStartDetails(args);
            }

            return _error;
        }

        private static void DisplayStartDetails(string[] args) => Console.WriteLine($"Arguments: {string.Join(", ", args)}");

        private async Task<int> Run(string[] args)
        {
            Console.WriteLine(_programTitle);
            Console.WriteLine();

            IOption option = new OptionBuilder()
                .AddCommandLine(args)
                .Build();

            if (option.Help)
            {
                option.GetHelp()
                    .Append(string.Empty)
                    .ForEach(x => Console.WriteLine(x));

                return _ok;
            }

            option.DumpConfigurations();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            using (ServiceProvider container = CreateContainer(option))
            {
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                    Console.WriteLine("Canceling...");
                };

                var activities = new Func<Task>[]
                {
                    () => option.Agent && option.Create ? container.GetRequiredService<AgentActivity>().Create(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.List ? container.GetRequiredService<AgentActivity>().List(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.Delete ? container.GetRequiredService<AgentActivity>().Delete(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.Clear ? container.GetRequiredService<AgentActivity>().Clear(cancellationTokenSource.Token) : Task.CompletedTask,

                    () => option.Target && option.Create ? container.GetRequiredService<TargetActivity>().Create(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.List ? container.GetRequiredService<AgentActivity>().List(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.Delete ? container.GetRequiredService<AgentActivity>().Delete(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.Clear ? container.GetRequiredService<AgentActivity>().Clear(cancellationTokenSource.Token) : Task.CompletedTask,

                    () => option.Balance ? container.GetRequiredService<BalanceActivity>().BalanceAgents(cancellationTokenSource.Token) : Task.CompletedTask,
                };

                await activities
                    .ForEachAsync(async x => await x());
            }

            Console.WriteLine();
            Console.WriteLine("Completed");
            return _ok;
        }

        private ServiceProvider CreateContainer(IOption option)
        {
            ServiceProvider container = new ServiceCollection()
                .AddHttpClient()
                .AddSingleton(option)
                .AddSingleton<ICosmosWatcherOption>(option.Store)
                .AddSingleton<IWatcherRepository, CosmosWatcherRepository>()
                .AddSingleton<AgentActivity>()
                .AddSingleton<TargetActivity>()
                .AddSingleton<BalanceActivity>()
                .BuildServiceProvider();

            return container;
        }
    }
}
