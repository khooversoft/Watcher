using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Services;
using Watcher.Cosmos.Repository;
using Watcher.Cosmos.Repository.Application;
using WatcherCmd.Activities;
using WatcherCmd.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;

[assembly: InternalsVisibleTo("WatcherCmd.Test")]

namespace WatcherCmd
{
    internal class Program
    {
        private const int _ok = 0;
        private const int _error = 1;
        private readonly string _programTitle = $"Watcher CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}";

        internal static async Task<int> Main(string[] args)
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
                .SetArgs(args)
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

            using (ServiceProvider serviceProvider = CreateContainer(option))
            {
                IServiceProvider container = serviceProvider;
                await InitializeRepository(container, cancellationTokenSource.Token);

                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                    Console.WriteLine("Canceling...");
                };

                var activities = new Func<Task>[]
                {
                    () => option.Agent && option.Create ? container.GetRequiredService<AgentActivity>().Create(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.Get ? container.GetRequiredService<AgentActivity>().Get(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.List ? container.GetRequiredService<AgentActivity>().List(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.Delete ? container.GetRequiredService<AgentActivity>().Delete(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.Clear ? container.GetRequiredService<AgentActivity>().Clear(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Agent && option.Template ? container.GetRequiredService<AgentActivity>().CreateTemplate(cancellationTokenSource.Token) : Task.CompletedTask,

                    () => option.Target && option.Create ? container.GetRequiredService<TargetActivity>().Create(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.Get ? container.GetRequiredService<TargetActivity>().Get(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.List ? container.GetRequiredService<TargetActivity>().List(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.Delete ? container.GetRequiredService<TargetActivity>().Delete(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.Clear ? container.GetRequiredService<TargetActivity>().Clear(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Target && option.Template ? container.GetRequiredService<TargetActivity>().CreateTemplate(cancellationTokenSource.Token) : Task.CompletedTask,

                    () => option.Assignment && option.Create ? container.GetRequiredService<AgentAssignmentActivity>().Create(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Assignment && option.List ? container.GetRequiredService<AgentAssignmentActivity>().List(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Assignment && option.Get ? container.GetRequiredService<AgentAssignmentActivity>().Get(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Assignment && option.Delete ? container.GetRequiredService<AgentAssignmentActivity>().Delete(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Assignment && option.Clear ? container.GetRequiredService<AgentAssignmentActivity>().Clear(cancellationTokenSource.Token) : Task.CompletedTask,
                    () => option.Assignment && option.Template ? container.GetRequiredService<AgentAssignmentActivity>().CreateTemplate(cancellationTokenSource.Token) : Task.CompletedTask,

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
                .AddLogging(config =>
                {
                    config
                        .AddConsole()
                        .AddDebug();

                    if (!option.LogFolder.IsEmpty()) config.AddLogFile(option.LogFolder!, "WatcherCmd");
                })
                .AddSingleton(option)
                .AddSingleton<ICosmosWatcherOption>(option.Store)
                .AddSingleton<IWatcherRepository, CosmosWatcherRepository>()
                .AddSingleton<IAgentController, AgentController>()
                .AddSingleton<IRecordContainer<AgentRecord>>(services =>
                {
                    IWatcherRepository watcherRepository = services.GetRequiredService<IWatcherRepository>();
                    return watcherRepository.Container.Get<AgentRecord>();
                })
                .AddSingleton<IRecordContainer<TargetRecord>>(services =>
                {
                    IWatcherRepository watcherRepository = services.GetRequiredService<IWatcherRepository>();
                    return watcherRepository.Container.Get<TargetRecord>();
                })
                .AddSingleton<IRecordContainer<AgentAssignmentRecord>>(services =>
                {
                    IWatcherRepository watcherRepository = services.GetRequiredService<IWatcherRepository>();
                    return watcherRepository.Container.Get<AgentAssignmentRecord>();
                })
                .AddSingleton<AgentActivity>()
                .AddSingleton<TargetActivity>()
                .AddSingleton<BalanceActivity>()
                .AddSingleton<AgentAssignmentActivity>()
                .AddSingleton<IJson, Json>()
                .BuildServiceProvider();

            return container;
        }

        private async Task InitializeRepository(IServiceProvider container, CancellationToken token)
        {
            Console.WriteLine("Initializing Cosmos Database");

            IWatcherRepository watcherRepository = container.GetRequiredService<IWatcherRepository>();
            await watcherRepository.InitializeEnvironment(token);
        }
    }
}
