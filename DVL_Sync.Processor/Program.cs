using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using DVL_Sync.Abstractions;
using DVL_Sync.Implementations;
using AltaSoft.Extensions.RepetitiveTask;

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

            //services.AddHostedRepetitiveTask("Execute",


        }
    }
}
