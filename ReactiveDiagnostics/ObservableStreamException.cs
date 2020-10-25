using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveDiagnostics
{
    /// <summary>
    /// Represents that an error was thrown in traced observable chain.
    /// </summary>
    public class ObservableStreamException : Exception
    {
        /// <summary>
        /// Gets the identifier of <see cref="DiagnosticsObservable.Trace"/> operator usings.
        /// </summary>
        public string OperatorKey { get; }

        /// <summary>
        /// Gets the stack trace at where <see cref="DiagnosticsObservable.Trace"/> operator was called.
        /// </summary>
        public string OperatorStackTrace { get; }

        /// <summary>
        /// Gets the stack trace at where the subscription was started.
        /// </summary>
        public string SubscriptionStackTrace { get; }

        public override string StackTrace => InnerException.StackTrace;

        public ObservableStreamException(Exception innerException, string key, string operatorStackTrace, string subscriptionStackTrace)
            :base("message", innerException)
        {
            OperatorKey = key;
            OperatorStackTrace = operatorStackTrace;
            SubscriptionStackTrace = subscriptionStackTrace;
        }
    }
}
