using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Xunit;

namespace ReactiveDiagnostics
{
    public class TraceTest
    {
        [Fact]
        public void Test1()
        {
            var result = new List<TraceRecord<int>>();
            var subject = new Subject<int>();
            var stream = subject
                .Trace("trace1", Observer.Create<TraceRecord<int>>(x => result.Add(x), error => { }));
            var subsc = stream.Subscribe(_ => { });
            foreach(var i in Enumerable.Range(0, 10))
            {
                subject.OnNext(i);
                Thread.Sleep(1000);
            }

            var comparer = new TraceRecordFuzzyComparer<int>(TimeSpan.FromSeconds(0.1), 0.01, true, null);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var res_entity = result.Where(x => x.SubscriptionIndex == 0).OrderBy(x => x.TotalDuration).ToList();

            Assert.Equal(result.Count, res_entity.Count);
            Assert.Equal(new[]
            {
                new TraceRecord<int>("trace1", 0, 0, "", "", threadId, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0), 0),
                new TraceRecord<int>("trace1", 0, 1, "", "", threadId, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 1),
                new TraceRecord<int>("trace1", 0, 2, "", "", threadId, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 2),
                new TraceRecord<int>("trace1", 0, 3, "", "", threadId, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1), 3),
                new TraceRecord<int>("trace1", 0, 4, "", "", threadId, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(1), 4),
                new TraceRecord<int>("trace1", 0, 5, "", "", threadId, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), 5),
                new TraceRecord<int>("trace1", 0, 6, "", "", threadId, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(1), 6),
                new TraceRecord<int>("trace1", 0, 7, "", "", threadId, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(1), 7),
                new TraceRecord<int>("trace1", 0, 8, "", "", threadId, TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(1), 8),
                new TraceRecord<int>("trace1", 0, 9, "", "", threadId, TimeSpan.FromSeconds(9), TimeSpan.FromSeconds(1), 9),
            }, res_entity, comparer);
        }


        [Fact]
        public void Test2()
        {
            var result = new List<TraceRecord<int>>();
            var subject = new Subject<int>();
            var stream = subject
                .Trace("trace1", Observer.Create<TraceRecord<int>>(x => result.Add(x), error => { }));
            var subsc1 = stream.Subscribe(_ => { });
            var subsc2 = stream.Subscribe(_ => { });
            foreach(var i in Enumerable.Range(0, 3))
            {
                subject.OnNext(i);
                Thread.Sleep(1000);
            }

            var comparer = new TraceRecordFuzzyComparer<int>(TimeSpan.FromSeconds(0.1), 0.01, true, null);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var res1_entity = result.Where(x => x.SubscriptionIndex == 0).OrderBy(x => x.TotalDuration).ToList();
            var res2_entity = result.Where(x => x.SubscriptionIndex == 1).OrderBy(x => x.TotalDuration).ToList();

            Assert.Equal(result.Count, res1_entity.Count + res2_entity.Count);
            Assert.Equal(new[]
            {
                new TraceRecord<int>("trace1", 0, 0, "", "", threadId, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0), 0),
                new TraceRecord<int>("trace1", 0, 1, "", "", threadId, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 1),
                new TraceRecord<int>("trace1", 0, 2, "", "", threadId, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 2),
            }, res1_entity, comparer);
            Assert.Equal(new[]
            {
                new TraceRecord<int>("trace1", 1, 0, "", "", threadId, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0), 0),
                new TraceRecord<int>("trace1", 1, 1, "", "", threadId, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 1),
                new TraceRecord<int>("trace1", 1, 2, "", "", threadId, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 2),
            }, res2_entity, comparer);
        }


        [Fact]
        public void Test3()
        {
            var result1 = new List<TraceRecord<int>>();
            var result2 = new List<TraceRecord<int>>();
            var subject = new Subject<int>();
            var stream = subject
                .Trace("trace1", Observer.Create<TraceRecord<int>>(x => result1.Add(x), error => { }))
                .Where(x => x % 2 == 0)
                .Trace("trace2", Observer.Create<TraceRecord<int>>(x => result2.Add(x), error => { }));
            var subsc = stream.Subscribe(_ => { });
            foreach(var i in Enumerable.Range(0, 5))
            {
                subject.OnNext(i);
                Thread.Sleep(1000);
            }

            var comparer = new TraceRecordFuzzyComparer<int>(TimeSpan.FromSeconds(0.1), 0.01, true, null);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Assert.Equal(new[]
            {
                new TraceRecord<int>("trace1", 0, 0, "", "", threadId, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0), 0),
                new TraceRecord<int>("trace1", 0, 1, "", "", threadId, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 1),
                new TraceRecord<int>("trace1", 0, 2, "", "", threadId, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 2),
                new TraceRecord<int>("trace1", 0, 3, "", "", threadId, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1), 3),
                new TraceRecord<int>("trace1", 0, 4, "", "", threadId, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(1), 4),
            }, result1, comparer);
            Assert.Equal(new[]
            {
                new TraceRecord<int>("trace2", 0, 0, "", "", threadId, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0), 0),
                new TraceRecord<int>("trace2", 0, 1, "", "", threadId, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2), 2),
                new TraceRecord<int>("trace2", 0, 2, "", "", threadId, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), 4),
            }, result2, comparer);
        }


        [Fact]
        public void Test4()
        {
            void waitWhile(Func<bool> isBlocked)
            {
                while(isBlocked())
                {
                    Thread.Sleep(1);
                }
            }

            var scheduler = new EventLoopScheduler();
            var threadId = -1;
            scheduler.Schedule(() => threadId = Thread.CurrentThread.ManagedThreadId);
            waitWhile(() => threadId < 0);

            var result = new List<TraceRecord<int>>();
            var subject = new Subject<int>();
            var stream = subject
                .ObserveOn(scheduler)
                .Trace("trace1", Observer.Create<TraceRecord<int>>(x => result.Add(x), error => { }));
            var count = 0;
            var subsc = stream.Subscribe(_ => Interlocked.Increment(ref count));
            foreach(var i in Enumerable.Range(0, 5))
            {
                subject.OnNext(i);
                Thread.Sleep(1000);
            }
            waitWhile(() => count < 5);

            var comparer = new TraceRecordFuzzyComparer<int>(TimeSpan.FromSeconds(0.1), 0.01, true, null);
            Assert.Equal(new[]
            {
                new TraceRecord<int>("trace1", 0, 0, "", "", threadId, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0), 0),
                new TraceRecord<int>("trace1", 0, 1, "", "", threadId, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 1),
                new TraceRecord<int>("trace1", 0, 2, "", "", threadId, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 2),
                new TraceRecord<int>("trace1", 0, 3, "", "", threadId, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1), 3),
                new TraceRecord<int>("trace1", 0, 4, "", "", threadId, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(1), 4),
            }, result, comparer);
        }
    }
}
