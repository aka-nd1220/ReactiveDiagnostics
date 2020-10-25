using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveDiagnostics
{
    internal static class Disposable
    {
        private sealed class EmptyDisposable : IDisposable
        {
            public static IDisposable Instance { get; } = new EmptyDisposable();
            private EmptyDisposable() { }
            public void Dispose() { }
        }

        public static IDisposable Empty => EmptyDisposable.Instance;
    }
}
