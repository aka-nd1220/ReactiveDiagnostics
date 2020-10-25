using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ReactiveDiagnostics
{
    internal unsafe ref struct StringBuilderSlim
    {
        public const int StackLimit = 16;

        private fixed char _stack[StackLimit];
        private TemporaryBuffer<char> _heap;

        public int Length { get; private set; }

        public ref char this[int i]
            => ref (_heap.HasBuffer ? ref _heap.Buffer[i] : ref _stack[i]);

        public void Dispose()
            => _heap.Dispose();

        public void Clear()
            => Length = 0;

        public void Add(char c)
        {
            var pos = Length;
            Resize(Length + 1);
            this[pos] = c;
        }

        public void Add(ReadOnlySpan<char> text)
        {
            var pos = Length;
            Resize(Length + text.Length);
            Unsafe.CopyBlock(
                ref Unsafe.As<char, byte>(ref this[pos]),
                ref Unsafe.As<char, byte>(ref Unsafe.AsRef(text[0])),
                (uint)(text.Length * Unsafe.SizeOf<char>()));
        }

        public new string ToString()
        {
            fixed(char* ptr = &this[0])
                return new string(ptr, 0, Length); 
        }

        private void Resize(int newLength)
        {
            if(_heap.HasBuffer
               || newLength <= StackLimit          // for the case of stack
               || newLength < _heap.Buffer.Length  // for the case of heap
               )
            {
                Length = newLength;
            }
            else if(Length <= StackLimit && StackLimit < newLength)
            {
                _heap = new TemporaryBuffer<char>(newLength);
                Unsafe.CopyBlock(
                    ref Unsafe.As<char, byte>(ref _heap.Buffer[0]),
                    ref Unsafe.As<char, byte>(ref _stack[0]),
                    (uint)(Length * Unsafe.SizeOf<char>()));
                Length = newLength;
            }
            else
            {
                var old = _heap;
                _heap = new TemporaryBuffer<char>(newLength);
                old.Buffer.Slice(0, Length).CopyTo(_heap.Buffer);
                Length = newLength;
                old.Dispose();
            }
        }
    }
}
