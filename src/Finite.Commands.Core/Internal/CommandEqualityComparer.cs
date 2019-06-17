using System;
using System.Collections.Generic;

namespace Finite.Commands
{
    internal sealed class CommandEqualityComparer
        : IEqualityComparer<ReadOnlyMemory<char>>
    {
        public static CommandEqualityComparer Default { get; }
            = new CommandEqualityComparer();

        private CommandEqualityComparer()
        { }

        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
        {
            return x.Span.SequenceEqual(y.Span);
        }

        public int GetHashCode(ReadOnlyMemory<char> obj)
        {
            var code = new HashCode();
            var span = obj.Span;

            for (int x = 0; x < obj.Length; x++)
            {
                code.Add(span[x]);
            }

            return code.ToHashCode();
        }
    }
}
