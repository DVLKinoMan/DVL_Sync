using DVL_Sync.Abstractions;
using DVL_Sync.Extensions;
using DVL_Sync.Implementations;
using DVL_Sync.Models;
//using AltaSoft.Extensions.RepetitiveTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
//using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Threading.Tasks;
//using DVL_Sync_FileEventsLogger.Models;

namespace DVL_Sync.Processor
{
    internal static class Program
    {
        internal static Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.RollingFile(Path.Combine(AppContext.BaseDirectory, "logs\\{Date}.txt"))
              .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
              .CreateLogger();

            try
            {
                var hostBuilder = Host.CreateDefaultBuilder()
                                      .ConfigureAppConfiguration(c => c.AddJsonFile("appSettings.json", false, true).Build())
                                      .ConfigureServices(ConfigureServices);

                return Environment.UserInteractive
                       ? hostBuilder.RunConsoleAsync()
                       : hostBuilder.UseServiceBaseLifetime().Build().RunAsync();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                logger.Dispose();
            }
        }

        private static void ConfigureServices(HostBuilderContext arg1, IServiceCollection services)
        {
            services.AddSingleton<IFolderOperationEventsReader, FolderOperationEventsReaderFromJsonFile>()
                    .AddSingleton<IFoldersSyncReader, FoldersSyncReaderFromJsonFile>()
                    .AddSingleton<IFoldersSyncer, FoldersSyncer>();


            services.AddSingleton<ILoggerFactory>(
                new Serilog.Extensions.Logging.SerilogLoggerFactory(
                    new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                        .WriteTo.RollingFile(Path.Combine(arg1.HostingEnvironment.ContentRootPath, "logs\\{Date}.txt"))
                        .CreateLogger(), true));

            IFoldersSyncer executor;

            //services.AddHostedRepetitiveTask("Execute",
            using (var sp = services.BuildServiceProvider())
            {
                executor = sp.GetRequiredService<IFoldersSyncer>();
            }

            services.AddHostedRepetitiveTask(c => executor.SyncFoldersAsync(arg1.Configuration.GetValue<string>("syncFoldersPath"), c), 
                new RepetitionOptions(arg1.Configuration.GetValue<TimeSpan>("executeInterval")));

        }
    }
}
