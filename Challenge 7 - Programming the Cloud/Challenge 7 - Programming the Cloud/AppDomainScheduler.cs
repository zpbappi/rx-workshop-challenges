using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Concurrency;
using System.Reflection;

namespace ProgrammingTheCloud
{
    class AppDomainScheduler : IScheduler
    {
        IScheduler scheduler;

        public AppDomainScheduler(string friendlyName)
            : this(AppDomain.CreateDomain(friendlyName))
        {
        }

        public AppDomainScheduler(AppDomain appDomain)
        {
            scheduler = (IScheduler)appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().CodeBase, typeof(RefScheduler).FullName);
        }

        public DateTimeOffset Now
        {
            get { return scheduler.Now; }
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, action);
        }

        class RefScheduler : MarshalByRefObject, IScheduler
        {
            IScheduler scheduler = new EventLoopScheduler();

            public DateTimeOffset Now
            {
                get { return scheduler.Now; }
            }

            public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
            {
                return new RefDisposable(scheduler.Schedule(state, dueTime, action));
            }

            public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
            {
                return new RefDisposable(scheduler.Schedule(state, dueTime, action));
            }

            public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
            {
                return new RefDisposable(scheduler.Schedule(state, action));
            }

            class RefDisposable : MarshalByRefObject, IDisposable
            {
                IDisposable disposable;

                public RefDisposable(IDisposable disposable)
                {
                    this.disposable = disposable;
                }

                public void Dispose()
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
