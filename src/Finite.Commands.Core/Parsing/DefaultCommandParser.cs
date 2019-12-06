using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Finite.Commands
{
    /// <summary>
    /// Default implementation of <see cref="ICommandParser{TContext}"/>
    /// which can be subclassed and overriden to provide enhanced features.
    /// </summary>
    public partial class DefaultCommandParser<TContext>
        : ICommandParser<TContext>
        where TContext : class, ICommandContext
    {
        private delegate (bool, object) ParserFunc(ReadOnlySpan<char> c);
        // A list of default parsers for TryParseObject.
        private static readonly Dictionary<Type, ParserFunc> _defaultParsers
            = new Dictionary<Type, ParserFunc>()
            {
                [typeof(sbyte)] = (x) => (sbyte.TryParse(x, out var y), y),
                [typeof(byte)] = (x) => (byte.TryParse(x, out var y), y),

                [typeof(short)] = (x) => (short.TryParse(x, out var y), y),
                [typeof(ushort)] = (x) => (ushort.TryParse(x, out var y), y),

                [typeof(int)] = (x) => (int.TryParse(x, out var y), y),
                [typeof(uint)] = (x) => (uint.TryParse(x, out var y), y),

                [typeof(long)] = (x) => (long.TryParse(x, out var y), y),
                [typeof(ulong)] = (x) => (ulong.TryParse(x, out var y), y)
            };
        private static readonly Type _stringType = typeof(string);

        /// <summary>
        /// Attempts to deserialize a parameter into a given type
        /// </summary>
        /// <param name="readerFactory">
        /// The type reader factory to request type readers from.
        /// </param>
        /// <param name="paramType">
        /// The parameter type to deserialize <paramref name="value"/> for.
        /// </param>
        /// <param name="value">
        /// A string containing the value of the parameter to deserialize.
        /// </param>
        /// <param name="result">
        /// The parsed result, boxed in an object.
        /// </param>
        /// <returns>
        /// A boolean indicating whether the parse was successful or not
        /// </returns>
        protected virtual bool TryParseObject(ITypeReaderFactory readerFactory,
            Type paramType, ReadOnlySpan<char> value,
            [NotNullWhen(true)]out object? result)
        {
            if (readerFactory.TryGetTypeReader(paramType, out var reader))
            {
                return reader.TryRead(value, out result);
            }
            else if (_defaultParsers.TryGetValue(paramType, out var parser))
            {
                var (success, parsed) = parser(value);
                result = parsed;
                return success;
            }
            else if (paramType == _stringType)
            {
                result = value.ToString();
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Dequotes a <see cref="ReadOnlySpan{T}"/> if it is quoted.
        /// </summary>
        /// <param name="value">
        /// The value to dequote.
        /// </param>
        /// <param name="result">
        /// The dequoted string.
        /// </param>
        /// <returns>
        /// <code>true</code> when the input span was quoted.
        /// </returns>
        protected virtual bool TryDequoteString(ReadOnlySpan<char> value,
            out ReadOnlySpan<char> result)
        {
            if (value.Length >= 2
                && IsCompletedQuote(value[0], value[value.Length - 1]))
            {
                result = value.Slice(1, value.Length - 2);
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Attempts to deserialize the arguments for a given comand match.
        /// </summary>
        /// <param name="executionContext">
        /// The execution context for the command.
        /// </param>
        /// <param name="match">
        /// The <see cref="CommandMatch"/> to deserialize arguments for.
        /// </param>
        /// <param name="result">
        /// The parsed arguments for this match.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> representing success
        /// </returns>
        protected virtual bool GetArgumentsForMatch(
            CommandExecutionContext executionContext, CommandMatch match,
            out object?[] result)
        {
            var readerFactory =
                executionContext.CommandService.TypeReaderFactory;

            var parameters = match.Command.Parameters;
            result = new object[parameters.Count];

            for (int i = 0; i < parameters.Count; i++)
            {
                var argument = parameters[i];

                if ((i == parameters.Count - 1) &&
                    argument.Attributes.Any(x => x is ParamArrayAttribute))
                {
                    if (!TryParseMultiple(argument, i, out var multiple))
                        return false;

                    result[i] = multiple;
                }
                else if (i >= match.Arguments.Length)
                {
                    if (!argument.Optional)
                        return false;

                    result[i] = argument.DefaultValue!;
                }
                else if (argument.Remainder)
                {
                    if (!TryParseRemainder(executionContext.Context.Message,
                        argument, match.Arguments[i].Span, out var remainder))
                        return false;

                    result[i] = remainder;
                }
                else
                {
                    var span = match.Arguments[i].Span;
                    if (TryDequoteString(span, out var tmp))
                        span = tmp;

                    var ok = TryParseObject(readerFactory,
                        argument.Type, span, out var value);

                    if (!ok)
                        return false;

                    result[i] = value;
                }
            }

            return true;

            bool TryParseMultiple(
                ParameterInfo argument, int startPos,
                out object?[] parsed)
            {
                var paramType = argument.Type.GetElementType();
                Debug.Assert(paramType != null);

                parsed = new object[match.Arguments.Length - startPos];
                for (int i = startPos; i < match.Arguments.Length; i++)
                {
                    var ok = TryParseObject(readerFactory,
                        paramType, match.Arguments[i].Span, out var value);

                    if (!ok)
                        return false;

                    parsed[i - startPos] = value;
                }

                return true;
            }

            bool TryParseRemainder(
                string fullMessage,
                ParameterInfo argument,
                ReadOnlySpan<char> param,
                out object parsed)
            {
                // Use damnit operator here as we can't use attributes in local
                // functions.
                if (!fullMessage.AsSpan().Overlaps(param, out var offset))
                {
                    parsed = null!;
                    return false;
                }

                return TryParseObject(readerFactory, argument.Type,
                    fullMessage.AsSpan().Slice(offset), out parsed!);
            }
        }

        /// <inheritdoc/>
        public virtual IResult Parse(CommandExecutionContext executionContext)
        {
            var result = Tokenize(executionContext.Context.Message,
                executionContext.PrefixLength);

            if (!result.IsSuccess)
                return result;

            ReadOnlyMemory<char>[] tokenStream = result.TokenStream!;
            var commands = executionContext.CommandService;

            foreach (var match in commands.FindCommands(tokenStream))
            {
                if (GetArgumentsForMatch(
                    executionContext,
                    match, out object?[] arguments))
                {
                    // TODO: maybe I should migrate this to a parser result?
                    executionContext.Command = match.Command;
                    executionContext.Arguments = arguments;

                    return SuccessResult.Instance;
                }
            }

            return CommandNotFoundResult.Instance;
        }
    }
}
