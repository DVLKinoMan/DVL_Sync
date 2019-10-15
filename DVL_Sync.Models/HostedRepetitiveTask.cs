using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DVL_Sync.Models
{
    public class HostedRepetitiveTask : IHostedService
    {
        private readonly Func<CancellationToken, Task> _func;
        private readonly RepetitionOptions _repetitionOptions;

        public HostedRepetitiveTask(Func<CancellationToken, Task> func, RepetitionOptions repetitionOptions) =>
            (this._func, this._repetitionOptions) = (func, repetitionOptions);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_repetitionOptions.Interval, cancellationToken);
                await _func(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
    }
}
