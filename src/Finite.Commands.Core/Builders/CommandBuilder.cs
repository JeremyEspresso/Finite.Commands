using System;
using System.Collections.Generic;

namespace Finite.Commands
{
    /// <summary>
    /// A builder class for creating <see cref="CommandInfo"/> instances.
    /// </summary>
    public sealed class CommandBuilder
    {
        // Aliases of the command
        private readonly List<string> _aliases;
        // Attributes of the command
        private readonly List<Attribute> _attributes;
        private readonly List<ParameterBuilder> _parameters;

        /// <summary>
        /// A collection of aliases applied to the
        /// <see cref="CommandInfo"/>.
        /// </summary>
        public IReadOnlyCollection<string> Aliases
            => _aliases.AsReadOnly();

        /// <summary>
        /// A collection of attributes applied to the
        /// <see cref="CommandInfo"/>.
        /// </summary>
        public IReadOnlyCollection<Attribute> Attributes
            => _attributes.AsReadOnly();

        /// <summary>
        /// A collection of parameters passed to this command.
        /// </summary>
        public IReadOnlyList<ParameterBuilder> Parameters
            => _parameters.AsReadOnly();

        /// <summary>
        /// The callback of the created <see cref="CommandInfo"/>.
        /// </summary>
        public CommandCallback Callback { get; set; }


#pragma warning disable CS8618
        // This constructor is only used by command builders and the public
        // ctor.
        internal CommandBuilder()
        {
            _aliases = new List<string>();
            _attributes = new List<Attribute>();
            _parameters = new List<ParameterBuilder>();
        }
#pragma warning restore CS8618

        /// <summary>
        /// Creates a new <see cref="CommandBuilder"/> with the specified
        /// callback.
        /// </summary>
        /// <param name="callback">
        /// The callback which is executed when the command is invoked.
        /// </param>
        public CommandBuilder(CommandCallback callback)
            : this()
        {
            Callback = callback;
        }

        /// <summary>
        /// Adds aliases to the created <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="aliases">
        /// The new aliases to add.
        /// </param>
        /// <returns>
        /// The current instance, for chaining calls.</returns>
        public CommandBuilder AddAliases(params string[] aliases)
        {
            _aliases.AddRange(aliases);
            return this;
        }

        /// <summary>
        /// Adds an attribute to the created <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="attribute">
        /// The attribute to add.
        /// </param>
        /// <returns>
        /// The current instance, for chaining calls.
        /// </returns>
        public CommandBuilder AddAttribute(Attribute attribute)
        {
            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds a parameter to the created <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="parameter">
        /// The parameter to add.
        /// </param>
        /// <returns>
        /// The current instance, for chaining calls.</returns>
        public CommandBuilder AddParameter(ParameterBuilder parameter)
        {
            _parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Sets the callback of the created <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="callback">
        /// The new callback to use.
        /// </param>
        /// <returns>
        /// The current instance, for chaining calls.
        /// </returns>
        public CommandBuilder WithCallback(CommandCallback callback)
        {
            Callback = callback;
            return this;
        }

        /// <summary>
        /// Builds a <see cref="CommandInfo"/> object with the given
        /// properties.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of context this command supports.
        /// </typeparam>
        /// <returns>
        /// The built command.
        /// </returns>
        public CommandInfo Build<TContext>()
            where TContext : class, ICommandContext
            => Build(null, typeof(TContext));

        internal CommandInfo Build(ModuleInfo? module, Type contextType)
        {
            return new CommandInfo(module, contextType, Callback,
                Aliases, Attributes, Parameters);
        }
    }
}
