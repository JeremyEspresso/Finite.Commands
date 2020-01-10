using System;
using System.Collections;
using System.Collections.Concurrent;

using static System.Numerics.BitOperations;

namespace Finite.Commands
{
    internal class BitArrayPool
    {
        public const int MaxPools = 17;

        public static BitArrayPool Default { get; } = new BitArrayPool(MaxPools);

        private readonly ConcurrentBag<BitArray>[] _pools;

        public BitArrayPool(int maxPools)
        {
            _pools = new ConcurrentBag<BitArray>[maxPools];

            for (var x = 0; x < maxPools; x++)
            {
                _pools[x] = new ConcurrentBag<BitArray>();
            }
        }

        public BitArray Rent(int minimumSize = 0)
        {
            if (minimumSize < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumSize));

            var poolIndex = 32 - LeadingZeroCount(
                unchecked((uint)(minimumSize - 1)));
            if (poolIndex > _pools.Length)
                poolIndex = _pools.Length - 1;

            var pool = _pools[poolIndex];

            if (!pool.TryTake(out var array))
                return new BitArray(1 << poolIndex);

            return array;
        }

        public void Return(BitArray array)
        {
            var poolIndex = 32 - LeadingZeroCount(
                unchecked((uint)array.Length - 1));

            var pool = _pools[poolIndex];

            pool.Add(array);
        }
    }
}
