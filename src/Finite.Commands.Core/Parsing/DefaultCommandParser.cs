using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            else if (paramType.IsArray)
            {
                return TryParseObject(readerFactory,
                    paramType.GetElementType()!, value, out result);
            }

            result = null;
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
            // Bail out early if we have no parameters
            if (match.Command.Parameters.Count == 0)
            {
                result = Array.Empty<object?>();
                return true;
            }

            var readerFactory =
                executionContext.CommandService.TypeReaderFactory;

            var parameters = match.Command.Parameters;
            var input = match.Message.AsMemory();
            result = new object?[parameters.Count];

            var position = 0;
            var argEnumerator = match.Arguments.GetEnumerator();
            var inputSpan = match.Message.AsSpan();

            while (position < parameters.Count)
            {
                var parameter = parameters[position];
                bool isParamArray = parameter.Attributes
                    .Any(x => x is ParamArrayAttribute);

                List<object?>? paramArrayBuilder = null;

                if (isParamArray)
                    paramArrayBuilder = new List<object?>();

                var hasArgument = argEnumerator.MoveNext();

                if (!hasArgument
                    && !isParamArray
                    && !parameter.Optional)
                    return false;

                else if (!hasArgument
                    && !isParamArray
                    && parameter.Optional)
                {
                    result[position] = parameter.DefaultValue;
                    position++;
                }

                else if (hasArgument)
                {
                    do
                    {
                        var range = argEnumerator.Current;

                        if (parameter.Remainder)
                        {
                            range = range.Start..^0;
                        }

                        var value = inputSpan[range];
                        var parsedObject = parameter.DefaultValue;

                        if (!TryParseObject(readerFactory, parameter.Type, value,
                            out parsedObject))
                            return false;

                        if (isParamArray)
                        {
                            paramArrayBuilder!.Add(parsedObject);
                        }
                        else
                        {
                            result[position] = parsedObject;
                            position++;
                            break;
                        }
                    }
                    while (argEnumerator.MoveNext());
                }

                if (isParamArray)
                {
                    result[position] = paramArrayBuilder!.ToArray();
                    position++;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public virtual IResult Parse(CommandExecutionContext executionContext)
        {
            var result = Tokenize(executionContext.Context.Message,
                executionContext.PrefixLength);

            if (!result.IsSuccess)
                return result;

            var commands = executionContext.CommandService;

            foreach (var match in commands.FindCommands(result))
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
