using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace ReactiveDiagnostics
{
    internal ref struct TemporaryBuffer<T>
    {
        private readonly T[] _array;
        public bool HasBuffer => _array is { };
        public Span<T> Buffer => _array;

        public TemporaryBuffer(int requiredLength)
            => _array = ArrayPool<T>.Shared.Rent(requiredLength);

        public void Dispose()
        {
            if(!HasBuffer)
                return;
            ArrayPool<T>.Shared.Return(_array);
        }
    }
}
