using System;

namespace Finite.Commands
{
    /// <summary>
    /// A typed implementation of <see cref="ITypeReader"/>, for implementation
    /// convenience.
    /// </summary>
    /// <typeparam name="T">
    /// The supported type which can be read.
    /// </typeparam>
    public abstract class TypeReader<T> : ITypeReader
    {
        /// <inheritdoc/>
        Type ITypeReader.SupportedType
            => typeof(T);

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
        public abstract bool TryRead(ReadOnlySpan<char> value, out T result);

        /// <inheritdoc/>
        bool ITypeReader.TryRead(ReadOnlySpan<char> value,
            out object? result)
        {
            var success = TryRead(value, out T realResult);
            result = realResult;
            return success;
        }
    }
}
