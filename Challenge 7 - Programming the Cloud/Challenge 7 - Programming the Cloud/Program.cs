using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ProgrammingTheCloud
{
    class Program
    {
        static void Main()
        {
            // TODO: Change this scheduler to use the AppDomainScheduler
            var scheduler = new AppDomainScheduler("MyDomain");

            Generate(0, x => x < 10, x => x + 1, x => x * x, Scheduler.ThreadPool)
                .ObserveLocally()
                .ForEach(Console.WriteLine);

            // Expected output:
            //   0
            //   1
            //   4
            //   9
            //   16
            //   25
            //   36
            //   49
            //   64
            //   81
        }

        static IObservable<R> Generate<T, R>(T initial, Func<T, bool> condition, Func<T, T> iterate, Func<T, R> resultSelector, IScheduler scheduler)
        {
            return new GenerateObservable<T, R>(initial, condition, iterate, resultSelector, scheduler);
        }

        class GenerateObservable<T, R> : IObservable<R>
        {
            T initial;
            Func<T, bool> condition;
            Func<T, T> iterate;
            Func<T, R> resultSelector;
            IScheduler scheduler;

            public GenerateObservable(T initial, Func<T, bool> condition, Func<T, T> iterate, Func<T, R> resultSelector, IScheduler scheduler)
            {
                this.initial = initial;
                this.condition = condition;
                this.iterate = iterate;
                this.resultSelector = resultSelector;
                this.scheduler = scheduler;
            }

            public IDisposable Subscribe(IObserver<R> observer)
            {
                // TODO: Rewrite this code to work in a distributed environment
                // HINT: Closures are bad! Try looking at the various Scheduler overloads.
                var current = initial;
                return scheduler.Schedule(self =>
                {
                    if (condition(current))
                    {
                        var result = resultSelector(current);
                        observer.OnNext(result);
                        current = iterate(current);
                        self();
                    }
                    else
                        observer.OnCompleted();
                });
            }

            [Serializable]
            class GenerateState
            {
                public T current;
                public Func<T, bool> condition;
                public Func<T, T> iterate;
                public Func<T, R> resultSelector;
                public IObserver<R> observer;
            }
        }
    }

    static class Ext
    {
        public static IObservable<T> ObserveLocally<T>(this IObservable<T> xs)
        {
            return new ObserveByRefObservable<T>(xs);
        }

        class ObserveByRefObservable<T> : IObservable<T>
        {
            IObservable<T> source;

            public ObserveByRefObservable(IObservable<T> source)
            {
                this.source = source;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return source.Subscribe(new RefObserver(observer));
            }

            class RefObserver : MarshalByRefObject, IObserver<T>
            {
                IObserver<T> observer;

                public RefObserver(IObserver<T> observer)
                {
                    this.observer = observer;
                }

                public void OnCompleted()
                {
                    observer.OnCompleted();
                }

                public void OnError(Exception error)
                {
                    observer.OnError(error);
                }

                public void OnNext(T value)
                {
                    observer.OnNext(value);
                }
            }
        }
    }
}
