using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveDiagnostics
{
    internal static class Observable
    {
        private sealed class Wrapper<T> : IObservable<T>
        {
            private readonly IObservable<T> _entity;
            public Wrapper(IObservable<T> entity) => _entity = entity;
            public IDisposable Subscribe(IObserver<T> observer) => _entity.Subscribe(observer);
        }

        public static IObservable<T> AsObservable<T>(this IObservable<T> observable)
            => new Wrapper<T>(observable);
    }
}
