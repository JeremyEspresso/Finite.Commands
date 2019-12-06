using System;
using System.Diagnostics.CodeAnalysis;

namespace Finite.Commands
{
    /// <summary>
    /// A factory used for creating instances of <see cref="ITypeReader"/>.
    /// </summary>
    public interface ITypeReaderFactory
    {
        /// <summary>
        /// Attempts to retrieve an untyped <see cref="ITypeReader"/>.
        /// </summary>
        /// <param name="valueType">
        /// The type the <see cref="ITypeReader"/> must be able to read.
        /// </param>
        /// <param name="reader">
        /// The found reader, if successful.
        /// </param>
        /// <returns>
        /// Returns <code>true</code> when successful, and <code>false</code>
        /// otherwise.
        /// </returns>
        bool TryGetTypeReader(Type valueType,
            [NotNullWhen(true)]out ITypeReader? reader);
    }
}
