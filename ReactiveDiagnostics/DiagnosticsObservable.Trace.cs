using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ReactiveDiagnostics
{
    partial class DiagnosticsObservable
    {
        private sealed class TraceObservable<T> : IObservable<T>
        {
            private sealed class Observer : IObserver<T>
            {
                private readonly IObserver<T> _observer;
                private readonly string _key;
                private readonly long _subscriptionIndex;
                private long _valueIndex;
                private readonly IObserver<TraceRecord<T>> _traceObserver;
                private readonly string _operatorStackTrace;
                private readonly string _subscriptionStackTrace;
                private readonly Stopwatch _stopwatch;
                private TimeSpan _prevLap;

                public Observer(IObserver<T> observer, string key, long subscriptionIndex, IObserver<TraceRecord<T>> traceObserver, string operatorStackTrace, string subscriptionStackTrace)
                {
                    _observer = observer;
                    _key = key;
                    _subscriptionIndex = subscriptionIndex;
                    _valueIndex = -1;
                    _traceObserver = traceObserver;
                    _operatorStackTrace = operatorStackTrace;
                    _subscriptionStackTrace = subscriptionStackTrace;
                    _stopwatch = new Stopwatch();
                    _stopwatch.Start();
                }

                public void OnNext(T value)
                {
                    var valueIndex = Interlocked.Increment(ref _valueIndex);
                    var duration = _stopwatch.Elapsed;
                    var record = new TraceRecord<T>(
                        _key,
                        _subscriptionIndex,
                        valueIndex,
                        _operatorStackTrace,
                        _subscriptionStackTrace,
                        Thread.CurrentThread.ManagedThreadId,
                        duration,
                        duration - _prevLap,
                        value);
                    _prevLap = duration;
                    _traceObserver.OnNext(record);
                    _observer.OnNext(value);
                }

                public void OnError(Exception error)
                {
                    _traceObserver.OnError(new ObservableStreamException(error, _key, _operatorStackTrace, _subscriptionStackTrace));
                    _observer.OnError(error);
                }

                public void OnCompleted()
                {
                    _traceObserver.OnCompleted();
                    _observer.OnCompleted();
                }
            }

            private readonly IObservable<T> _observable;
            private readonly string _operatorKey;
            private long _subscriptionIndex;
            private readonly string _operatorStackTrace;
            private readonly IObserver<TraceRecord<T>> _traceObserver;

            public TraceObservable(IObservable<T> observable, string operatorKey, string operatorStackTrace, IObserver<TraceRecord<T>> traceObserver)
            {
                _observable = observable;
                _operatorKey = operatorKey;
                _subscriptionIndex = -1;
                _operatorStackTrace = operatorStackTrace;
                _traceObserver = traceObserver;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                var subscriptionStackTrace = Environment.StackTrace;
                var subscriptionIndex = Interlocked.Increment(ref _subscriptionIndex);
                var newObserver = new Observer(observer, _operatorKey, subscriptionIndex, _traceObserver, _operatorStackTrace, subscriptionStackTrace);
                return _observable.Subscribe(newObserver);
            }
        }

        /// <summary>
        /// Inserts trace point into observable stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"> The source stream. </param>
        /// <param name="operatorKey"> The key to determine this trace point. </param>
        /// <param name="traceObserver"> The observer that will be called when some kind of stream event will pass this trace point. </param>
        /// <returns></returns>
        public static IObservable<T> Trace<T>(this IObservable<T> observable, string operatorKey, IObserver<TraceRecord<T>> traceObserver)
        {
            if(observable is null)
                throw new ArgumentNullException(nameof(observable));
            if(operatorKey is null)
                throw new ArgumentNullException(nameof(operatorKey));
            if(traceObserver is null)
                throw new ArgumentNullException(nameof(traceObserver));

            var operatorStackTrace = Environment.StackTrace;
            return new TraceObservable<T>(observable, operatorKey, operatorStackTrace, traceObserver);
        }

        /// <summary>
        /// Inserts trace point into observable stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"> The source stream. </param>
        /// <param name="operatorKey"> The key to determine this trace point. </param>
        /// <param name="onNextTrace"> The action that will be called when a value will pass this trace point. </param>
        /// <returns></returns>
        public static IObservable<T> Trace<T>(this IObservable<T> observable, string operatorKey, Action<TraceRecord<T>> onNextTrace)
        {
            if(observable is null)
                throw new ArgumentNullException(nameof(observable));
            if(operatorKey is null)
                throw new ArgumentNullException(nameof(operatorKey));
            if(onNextTrace is null)
                throw new ArgumentNullException(nameof(onNextTrace));

            var operatorStackTrace = Environment.StackTrace;
            var traceObserver = Observer.Create(onNextTrace);
            return new TraceObservable<T>(observable, operatorKey, operatorStackTrace, traceObserver);
        }

        /// <summary>
        /// Inserts trace point into observable stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"> The source stream. </param>
        /// <param name="operatorKey"> The key to determine this trace point. </param>
        /// <param name="onError"> The action that will be called when an error will pass this trace point. </param>
        /// <returns></returns>
        public static IObservable<T> Trace<T>(this IObservable<T> observable, string operatorKey, Action<Exception> onError)
        {
            if(observable is null)
                throw new ArgumentNullException(nameof(observable));
            if(operatorKey is null)
                throw new ArgumentNullException(nameof(operatorKey));
            if(onError is null)
                throw new ArgumentNullException(nameof(onError));

            var operatorStackTrace = Environment.StackTrace;
            var traceObserver = Observer.Create<TraceRecord<T>>(onError);
            return new TraceObservable<T>(observable, operatorKey, operatorStackTrace, traceObserver);
        }


        /// <summary>
        /// Inserts trace point into observable stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"> The source stream. </param>
        /// <param name="operatorKey"> The key to determine this trace point. </param>
        /// <param name="onNextTrace"> The action that will be called when a value will pass this trace point. </param>
        /// <param name="onError"> The observer that will be called when an error will pass this trace point. </param>
        /// <returns></returns>
        public static IObservable<T> Trace<T>(this IObservable<T> observable, string operatorKey, Action<TraceRecord<T>> onNextTrace, Action<Exception> onError)
        {
            if(observable is null)
                throw new ArgumentNullException(nameof(observable));
            if(operatorKey is null)
                throw new ArgumentNullException(nameof(operatorKey));
            if(onNextTrace is null)
                throw new ArgumentNullException(nameof(onNextTrace));
            if(onError is null)
                throw new ArgumentNullException(nameof(onError));

            var operatorStackTrace = Environment.StackTrace;
            var traceObserver = Observer.Create(onNextTrace, onError);
            return new TraceObservable<T>(observable, operatorKey, operatorStackTrace, traceObserver);
        }

        /// <summary>
        /// Inserts trace point into observable stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"> The source stream. </param>
        /// <param name="operatorKey"> The key to determine this trace point. </param>
        /// <param name="traceObservable"> The observable instance to subscribe trace event. </param>
        /// <returns></returns>
        public static IObservable<T> Trace<T>(this IObservable<T> observable, string operatorKey, out IObservable<TraceRecord<T>> traceObservable)
        {
            if(observable is null)
                throw new ArgumentNullException(nameof(observable));
            if(operatorKey is null)
                throw new ArgumentNullException(nameof(operatorKey));

            var operatorStackTrace = Environment.StackTrace;
            var subject = new Subject<TraceRecord<T>>();
            traceObservable = subject.AsObservable();
            return new TraceObservable<T>(observable, operatorKey, operatorStackTrace, subject);
        }

        /// <summary>
        /// Inserts trace point into observable stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"> The source stream. </param>
        /// <param name="operatorKey"> The key to determine this trace point. </param>
        /// <param name="logger"> The action to output trace informations. </param>
        /// <returns></returns>
        public static IObservable<T> Trace<T>(this IObservable<T> observable, string operatorKey, Action<string> logger)
        {
            if(observable is null)
                throw new ArgumentNullException(nameof(observable));
            if(operatorKey is null)
                throw new ArgumentNullException(nameof(operatorKey));
            if(logger is null)
                throw new ArgumentNullException(nameof(logger));

            var operatorStackTrace = Environment.StackTrace;
            var traceObserver = Observer.Create<TraceRecord<T>>(record => logger(record.ToString()), error => logger(error.ToString()));
            return new TraceObservable<T>(observable, operatorKey, operatorStackTrace, traceObserver);
        }
    }
}
