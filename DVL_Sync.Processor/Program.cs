using DVL_Sync.Abstractions;
using DVL_Sync.Extensions;
using DVL_Sync.Implementations;
using DVL_Sync.Models;
//using AltaSoft.Extensions.RepetitiveTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Processor
{
    internal static class Program
    {
        internal static Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                                  .ConfigureServices(ConfigureServices);

            return Environment.UserInteractive
                   ? hostBuilder.RunConsoleAsync()
                   : hostBuilder.UseServiceBaseLifetime().Build().RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext arg1, IServiceCollection services)
        {
            services.AddSingleton<IFolderOperationEventsReader, FolderOperationEventsReaderFromJsonFile>()
                    .AddSingleton<IFoldersSyncer, FoldersSyncer>();

            IFoldersSyncer executor;

            //services.AddHostedRepetitiveTask("Execute",
            using (var sp = services.BuildServiceProvider())
            {
                executor = sp.GetRequiredService<IFoldersSyncer>();
            }

            //Todo???
            services.AddHostedRepetitiveTask(c => executor.SyncFoldersAsync(c, new FolderConfig(), new FolderConfig()), new RepetitionOptions(arg1.Configuration.GetValue<TimeSpan>("executeInterval")));

        }
    }
}
