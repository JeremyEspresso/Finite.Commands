using System;

namespace Finite.Commands
{
    /// <summary>
    /// A class used for containing command matches.
    /// </summary>
    public sealed class CommandMatch
    {
        internal CommandMatch(string message, CommandInfo command,
            IndexSet arguments,
            IndexSet path)
        {
            Message = message;
            Command = command;
            Arguments = arguments;
            CommandPath = path;
        }

        /// <summary>
        /// The message this match was for.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The command matched by this match
        /// </summary>
        public CommandInfo Command { get; }

        /// <summary>
        /// The list of arguments to pass to this matched command
        /// </summary>
        public IndexSet Arguments { get; }

        /// <summary>
        /// The full path of the command which was matched
        /// </summary>
        public IndexSet CommandPath { get; }
    }
}
