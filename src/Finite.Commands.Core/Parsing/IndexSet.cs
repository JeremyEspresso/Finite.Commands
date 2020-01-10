using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Finite.Commands
{
    /// <summary>
    /// An ordered set of ranges.
    /// </summary>
    public sealed class IndexSet : IEnumerable<Range>, IDisposable
    {
        private readonly BitArray _backingArray;

        /// <summary>
        /// Gets the reference length for this index set.
        /// </summary>
        public int ReferenceLength { get; }

        /// <summary>
        /// Creates a new <see cref="IndexSet"/> with the given reference
        /// length.
        /// </summary>
        /// <param name="length">
        /// The reference length of the index set.
        /// </param>
        public IndexSet(int length)
        {
            ReferenceLength = length;
            _backingArray = BitArrayPool.Default.Rent(length);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            BitArrayPool.Default.Return(_backingArray);
        }

        /// <summary>
        /// Adds a range to the index set.
        /// </summary>
        /// <param name="range">
        /// The range to add.
        /// </param>
        public void AddRange(Range range)
        {
            var (start, length) = range.GetOffsetAndLength(ReferenceLength);

            for (int x = 0; x < length; x++)
                _backingArray.Set(start + x, true);
        }

        /// <summary>
        /// Adds an index to the index set.
        /// </summary>
        /// <param name="index">
        /// The index to add.
        /// </param>
        public void Add(Index index)
        {
            var offset = index.GetOffset(ReferenceLength);
            _backingArray.Set(offset, true);
        }

        /// <summary>
        /// Removes a range from the index set.
        /// </summary>
        /// <param name="range">
        /// The range to remove.
        /// </param>
        public void RemoveRange(Range range)
        {
            var (start, length) = range.GetOffsetAndLength(ReferenceLength);
            for (int x = 0; x < length; x++)
                _backingArray.Set(start + x, true);
        }

        /// <summary>
        /// Removes an index from the index set.
        /// </summary>
        /// <param name="index">
        /// The index to remove.
        /// </param>
        public void Remove(Index index)
        {
            var offset = index.GetOffset(ReferenceLength);
            _backingArray.Set(offset, false);
        }

        /// <inheritdoc/>
        public IEnumerator<Range> GetEnumerator()
        {
            int start = 0;

            var previous = false;
            for (int i = 0; i < ReferenceLength; i++)
            {
                var current = _backingArray[i];

                if (current != previous && current)
                    start = i;
                else if (current != previous && !current)
                {
                    yield return start..i;
                }

                previous = current;
            }

            if (previous)
                yield return start..ReferenceLength;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
