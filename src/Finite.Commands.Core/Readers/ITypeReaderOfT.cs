using System;

namespace Finite.Commands
{
    public interface ITypeReader<T> : ITypeReader
    {
        bool TryRead(ReadOnlySpan<char> value, out T result);
    }
}
