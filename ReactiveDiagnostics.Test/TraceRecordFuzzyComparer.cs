using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ReactiveDiagnostics
{
    class TraceRecordFuzzyComparer<T> : IEqualityComparer<TraceRecord<T>>
    {
        private readonly TimeSpan _timeSpanAbsoluteEpsilon;
        private readonly double _timeSpanRelativeEpsilon;
        private readonly bool _checksThread;
        private readonly EqualityComparer<string>? _stackTraceComparer;

        public TraceRecordFuzzyComparer(TimeSpan timeSpanAbsoluteEpsilon, double timeSpanRelativeEpsilon, bool checksThread, EqualityComparer<string>? stackTraceComparer)
        {
            _timeSpanAbsoluteEpsilon = timeSpanAbsoluteEpsilon;
            _timeSpanRelativeEpsilon = timeSpanRelativeEpsilon;
            _checksThread = checksThread;
            _stackTraceComparer = stackTraceComparer;
        }

        public bool Equals([AllowNull] TraceRecord<T> x, [AllowNull] TraceRecord<T> y)
        {
            if(x is null && y is null)
                return true;
            if(x is null || y is null)
                return false;
            if(x.OperatorKey != y.OperatorKey)
                return false;
            if(x.SubscriptionIndex != y.SubscriptionIndex)
                return false;
            if(x.ValueIndex != y.ValueIndex)
                return false;
            if(_checksThread && x.ManagedThreadId != y.ManagedThreadId)
                return false;
            if(_stackTraceComparer is { } && !_stackTraceComparer.Equals(x.OperatorStackTrace, y.OperatorStackTrace))
                return false;
            if(_stackTraceComparer is { } && !_stackTraceComparer.Equals(x.SubscriptionStackTrace, y.SubscriptionStackTrace))
                return false;
            if(!FuzzyEquals(x.TotalDuration, y.TotalDuration))
                return false;
            if(!FuzzyEquals(x.LapDuration, y.LapDuration))
                return false;
            if(!EqualityComparer<T>.Default.Equals(x.Value, y.Value))
                return false;
            return true;
        }

        public int GetHashCode([DisallowNull] TraceRecord<T> obj) => 0;

        private bool FuzzyEquals(TimeSpan x, TimeSpan y)
            => Math.Abs(x.TotalSeconds - y.TotalSeconds) < _timeSpanAbsoluteEpsilon.TotalSeconds
            || Math.Abs(x.TotalSeconds - y.TotalSeconds) / (Math.Abs(x.TotalSeconds) + Math.Abs(y.TotalSeconds)) < _timeSpanRelativeEpsilon;
    }
}
