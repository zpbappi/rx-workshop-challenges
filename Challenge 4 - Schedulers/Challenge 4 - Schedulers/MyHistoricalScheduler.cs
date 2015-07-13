using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Threading;

namespace Schedulers
{
    class MyHistoricalScheduler : HistoricalScheduler
    {
        public void Run(TimeSpan delay)
        {
            Scheduler.ThreadPool.Schedule(
                delay,
                self =>
                {
                    var next = GetNext();
                    if (next == null)
                        return;
                    var dt = next.DueTime;
                    AdvanceTo(dt);
                    self(delay);
                });
        }
    }
}
