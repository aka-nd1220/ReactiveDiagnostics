using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveDiagnostics
{
    internal static class Observer
    {
        private sealed class ObserverN<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;

            public ObserverN(Action<T> onNext)
                => _onNext = onNext;

            public void OnNext(T value) => _onNext(value);
            public void OnCompleted() { }
            public void OnError(Exception error) { }
        }

        private sealed class ObserverE<T> : IObserver<T>
        {
            private readonly Action<Exception> _onError;

            public ObserverE(Action<Exception> onError)
                => _onError = onError;

            public void OnNext(T value) { }
            public void OnCompleted() { }
            public void OnError(Exception error) => _onError(error);
        }

        private sealed class ObserverNE<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            private readonly Action<Exception> _onError;

            public ObserverNE(Action<T> onNext, Action<Exception> onError)
                => (_onNext, _onError) = (onNext, onError);

            public void OnNext(T value) => _onNext(value);
            public void OnCompleted() { }
            public void OnError(Exception error) => _onError(error);
        }


        public static IObserver<T> Create<T>(Action<T> onNext)
            => new ObserverN<T>(onNext);


        public static IObserver<T> Create<T>(Action<Exception> onrror)
            => new ObserverE<T>(onrror);


        public static IObserver<T> Create<T>(Action<T> onNext, Action<Exception> onError)
            => new ObserverNE<T>(onNext, onError);
    }
}
