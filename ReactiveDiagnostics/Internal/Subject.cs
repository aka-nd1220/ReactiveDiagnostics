using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ReactiveDiagnostics
{
    // This class was ported from <a href="https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Subjects/Subject.cs">Rx.Net</a>.
    internal sealed class Subject<T> : IObservable<T>, IObserver<T>, IDisposable
    {
        private static readonly SubjectDisposable[] Terminated = new SubjectDisposable[0];
        private static readonly SubjectDisposable[] Disposed = new SubjectDisposable[0];

        private SubjectDisposable[] _observers = Array.Empty<SubjectDisposable>();
        private Exception? _exception;

        private static void ThrowDisposed() => throw new ObjectDisposedException(string.Empty);

        public void OnCompleted()
        {
            for(; ; )
            {
                var observers = Volatile.Read(ref _observers);

                if(observers == Disposed)
                {
                    _exception = null;
                    ThrowDisposed();
                    break;
                }

                if(observers == Terminated)
                {
                    break;
                }

                if(Interlocked.CompareExchange(ref _observers, Terminated, observers) == observers)
                {
                    foreach(var observer in observers)
                    {
                        observer.Observer?.OnCompleted();
                    }

                    break;
                }
            }
        }

        public void OnError(Exception error)
        {
            if(error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            for(; ; )
            {
                var observers = Volatile.Read(ref _observers);

                if(observers == Disposed)
                {
                    _exception = null;
                    ThrowDisposed();
                    break;
                }

                if(observers == Terminated)
                {
                    break;
                }

                _exception = error;

                if(Interlocked.CompareExchange(ref _observers, Terminated, observers) == observers)
                {
                    foreach(var observer in observers)
                    {
                        observer.Observer?.OnError(error);
                    }

                    break;
                }
            }
        }

        public void OnNext(T value)
        {
            var observers = Volatile.Read(ref _observers);

            if(observers == Disposed)
            {
                _exception = null;
                ThrowDisposed();
                return;
            }

            foreach(var observer in observers)
            {
                observer.Observer?.OnNext(value);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if(observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var disposable = default(SubjectDisposable);
            for(; ; )
            {
                var observers = Volatile.Read(ref _observers);

                if(observers == Disposed)
                {
                    _exception = null;
                    ThrowDisposed();

                    break;
                }

                if(observers == Terminated)
                {
                    var ex = _exception;

                    if(ex != null)
                    {
                        observer.OnError(ex);
                    }
                    else
                    {
                        observer.OnCompleted();
                    }

                    break;
                }

                disposable ??= new SubjectDisposable(this, observer);

                var n = observers.Length;
                var b = new SubjectDisposable[n + 1];

                Array.Copy(observers, 0, b, 0, n);

                b[n] = disposable;

                if(Interlocked.CompareExchange(ref _observers, b, observers) == observers)
                {
                    return disposable;
                }
            }

            return Disposable.Empty;
        }

        private void Unsubscribe(SubjectDisposable observer)
        {
            for(; ; )
            {
                var a = Volatile.Read(ref _observers);
                var n = a.Length;

                if(n == 0)
                {
                    break;
                }

                var j = Array.IndexOf(a, observer);

                if(j < 0)
                {
                    break;
                }

                SubjectDisposable[] b;

                if(n == 1)
                {
                    b = Array.Empty<SubjectDisposable>();
                }
                else
                {
                    b = new SubjectDisposable[n - 1];

                    Array.Copy(a, 0, b, 0, j);
                    Array.Copy(a, j + 1, b, j, n - j - 1);
                }

                if(Interlocked.CompareExchange(ref _observers, b, a) == a)
                {
                    break;
                }
            }
        }

        private sealed class SubjectDisposable : IDisposable
        {
            private Subject<T> _subject;
            private volatile IObserver<T>? _observer;

            public SubjectDisposable(Subject<T> subject, IObserver<T> observer)
            {
                _subject = subject;
                _observer = observer;
            }

            public IObserver<T>? Observer => _observer;

            public void Dispose()
            {
                var observer = Interlocked.Exchange(ref _observer, null);
                if(observer == null)
                {
                    return;
                }

                _subject.Unsubscribe(this);
                _subject = null!;
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _observers, Disposed);
            _exception = null;
        }
    }
}
