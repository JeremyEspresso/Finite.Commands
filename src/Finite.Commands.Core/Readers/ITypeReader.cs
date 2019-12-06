using System;
using System.Diagnostics.CodeAnalysis;

namespace Finite.Commands
{
    /// <summary>
    /// An interface used for reading types from strings.
    /// </summary>
    public interface ITypeReader
    {
        /// <summary>
        /// Gets the supported type of this type reader
        /// </summary>
        Type SupportedType { get; }

        /// <summary>
        /// Attempts to read the given value.
        /// </summary>
        /// <param name="value">
        /// The value to read.
        /// </param>
        /// <param name="result">
        /// The parsed value, if successful.
        /// </param>
        /// <returns>
        /// Returns <code>true</code> when reading was successful, and
        /// <code>false</code> otherwise.
        /// </returns>
        bool TryRead(ReadOnlySpan<char> value,
            [NotNullWhen(true)]out object? result);
    }
}
