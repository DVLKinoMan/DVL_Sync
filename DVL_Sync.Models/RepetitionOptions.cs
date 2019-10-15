using System;

namespace DVL_Sync.Models
{
    public class RepetitionOptions
    {
        public TimeSpan Interval { get; }

        public RepetitionOptions(TimeSpan interval) => this.Interval = interval;

    }
}
