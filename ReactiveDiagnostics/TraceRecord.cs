using System;

namespace ReactiveDiagnostics
{
    /// <summary>
    /// Type-unspecified interface of <see cref="TraceRecord{T}"/>.
    /// </summary>
    public interface ITraceRecord : IFormattable
    {
        /// <summary>
        /// Gets the key which can determine where <see cref="DiagnosticsObservable.Trace"/> operator was called.
        /// </summary>
        public string OperatorKey { get; }

        /// <summary>
        /// Gets the order index of the subscription in the stream source.
        /// </summary>
        public long SubscriptionIndex { get; }

        /// <summary>
        /// Gets the order index of the value in the stream subscription.
        /// This means the count how much times <see cref="IObserver{T}.OnNext(T)"/> was called.
        /// </summary>
        public long ValueIndex { get; }

        /// <summary>
        /// Gets the stack trace at where <see cref="DiagnosticsObservable.Trace"/> operator was called.
        /// </summary>
        public string OperatorStackTrace { get; }

        /// <summary>
        /// Gets the stack trace at where the subscription was started.
        /// </summary>
        public string SubscriptionStackTrace { get; }

        /// <summary>
        /// Gets the thread ID who dealed this item.
        /// </summary>
        public int ManagedThreadId { get; }

        /// <summary>
        /// Gets the total duration from starting subscription of the stream.
        /// </summary>
        public TimeSpan TotalDuration { get; }

        /// <summary>
        /// Gets the lap duration from previous item.
        /// </summary>
        public TimeSpan LapDuration { get; }

        /// <summary>
        /// Gets the value of this recorded item.
        /// </summary>
        public object? Value { get; }
    }

    /// <summary>
    /// Contains a recorded properties in the observable stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class TraceRecord<T> : ITraceRecord
    {
        /// <summary>
        /// Gets the key which can determine where <see cref="DiagnosticsObservable.Trace"/> operator was called.
        /// </summary>
        public string OperatorKey { get; }

        /// <summary>
        /// Gets the order index of the subscription in the stream source.
        /// </summary>
        public long SubscriptionIndex { get; }

        /// <summary>
        /// Gets the order index of the value in the stream subscription.
        /// This means the count how much times <see cref="IObserver{T}.OnNext(T)"/> was called.
        /// </summary>
        public long ValueIndex { get; }

        /// <summary>
        /// Gets the stack trace at where <see cref="DiagnosticsObservable.Trace"/> operator was called.
        /// </summary>
        public string OperatorStackTrace { get; }

        /// <summary>
        /// Gets the stack trace at where the subscription was started.
        /// </summary>
        public string SubscriptionStackTrace { get; }

        /// <summary>
        /// Gets the thread ID who dealed this item.
        /// </summary>
        public int ManagedThreadId { get; }

        /// <summary>
        /// Gets the total duration from starting subscription of the stream.
        /// </summary>
        public TimeSpan TotalDuration { get; }

        /// <summary>
        /// Gets the lap duration from previous item.
        /// </summary>
        public TimeSpan LapDuration { get; }

        /// <summary>
        /// Gets the value of this recorded item.
        /// </summary>
        public T Value { get; }
        object? ITraceRecord.Value => Value;

        /// <summary>
        /// Initializes a new instance of <see cref="TraceRecord{T}"/>.
        /// </summary>
        /// <param name="operatorKey"></param>
        /// <param name="subscriotionIndex"></param>
        /// <param name="valueIndex"></param>
        /// <param name="operatorStackTrace"></param>
        /// <param name="subscriptionStackTrace"></param>
        /// <param name="managedThreadId"></param>
        /// <param name="totalDuration"></param>
        /// <param name="lapDuration"></param>
        /// <param name="value"></param>
        public TraceRecord(string operatorKey, long subscriotionIndex, long valueIndex, string operatorStackTrace, string subscriptionStackTrace, int managedThreadId, TimeSpan totalDuration, TimeSpan lapDuration, T value)
        {
            OperatorKey = operatorKey;
            SubscriptionIndex = subscriotionIndex;
            ValueIndex = valueIndex;
            OperatorStackTrace = operatorStackTrace;
            SubscriptionStackTrace = subscriptionStackTrace;
            ManagedThreadId = managedThreadId;
            TotalDuration = totalDuration;
            LapDuration = lapDuration;
            Value = value;
        }
    }
}
