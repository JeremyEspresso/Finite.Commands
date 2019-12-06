using System;

namespace Finite.Commands.Tests
{
    public class NullTypeReaderFactory : ITypeReaderFactory
    {
        public bool TryGetTypeReader(Type valueType, out ITypeReader? reader)
        {
            reader = default;
            return false;
        }
    }
}
