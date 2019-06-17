using System;

namespace Finite.Commands
{
    public interface ITypeReader
    {
        Type SupportedType { get; }
        bool TryRead(ReadOnlySpan<char> value, out object result);
    }
}
