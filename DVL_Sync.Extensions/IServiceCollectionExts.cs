using DVL_Sync.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DVL_Sync.Extensions
{
    public static class IServiceCollectionExts
    {
        public static IServiceCollection AddHostedRepetitiveTask(this IServiceCollection self,
            Func<CancellationToken, Task> repetitiveTask,
            RepetitionOptions repetitionOptions)
        => self.AddHostedRepetitiveTask(new HostedRepetitiveTask(repetitiveTask, repetitionOptions));

        public static IServiceCollection AddHostedRepetitiveTask(this IServiceCollection self, HostedRepetitiveTask rt) =>
                self.AddSingleton(rt)
                .AddSingleton<IHostedService>(rt);
    }
}
