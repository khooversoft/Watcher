using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Watcher.Cosmos.Repository;
using WatcherAgent.Services;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using WatcherSdk.Services.State;

namespace WatcherAgent
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IRunningStateService, RunningStateService>();
            services.AddSingleton<IWatcherRepository, CosmosWatcherRepository>();
            services.AddSingleton<IAgentController, AgentController>();
            services.AddSingleton<JobHost>();

            services.AddSingleton<IRecordContainer<AgentRecord>>(services =>
            {
                IWatcherRepository watcherRepository = services.GetRequiredService<IWatcherRepository>();
                return watcherRepository.Container.Get<AgentRecord>();
            });

            services.AddSingleton<IRecordContainer<TargetRecord>>(services =>
            {
                IWatcherRepository watcherRepository = services.GetRequiredService<IWatcherRepository>();
                return watcherRepository.Container.Get<TargetRecord>();
            });

            services.AddSingleton<IRecordContainer<TraceRecord>>(services =>
            {
                IWatcherRepository watcherRepository = services.GetRequiredService<IWatcherRepository>();
                return watcherRepository.Container.Get<TraceRecord>();
            });

            services.AddHostedService<JobHost>();
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
