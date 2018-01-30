﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Wumpus.Commands
{
    /// <summary>
    /// A builder class for creating Modules.
    /// </summary>
    public class ModuleBuilder
    {
        // Prefixes that this module adds to a command
        private readonly List<string> _aliases;
        // Submodules of this module
        private readonly List<ModuleBuilder> _submodules;
        // Attributes of this module
        private readonly List<Attribute> _attributes;
        // Commands of this module
        private readonly List<CommandBuilder> _commands;

        /// <summary>
        /// A collection of aliases applied to this module.
        /// </summary>
        public IReadOnlyCollection<string> Aliases
            => _aliases.AsReadOnly();

        /// <summary>
        /// A collection of submodules contained by this module.
        /// </summary>
        public IReadOnlyCollection<ModuleBuilder> Submodules
            => _submodules.AsReadOnly();

        /// <summary>
        /// A collection of attributes applied to this command.
        /// </summary>
        public IReadOnlyCollection<Attribute> Attributes
            => _attributes.AsReadOnly();

        public IReadOnlyCollection<CommandBuilder> Commands
            => _commands.AsReadOnly();

        /// <summary>
        /// The parent of the created Module.
        /// </summary>
        public ModuleBuilder Parent { get; internal set; }

        /// <summary>
        /// Creates a new ModuleBuilder.
        /// </summary>
        public ModuleBuilder()
        {
            _aliases = new List<string>();
            _submodules = new List<ModuleBuilder>();
            _attributes = new List<Attribute>();
            _commands = new List<CommandBuilder>();
        }

        /// <summary>
        /// Adds aliases to the created Module
        /// </summary>
        /// <param name="aliases">The new aliases to add</param>
        /// <returns>The current instance, for chaining calls</returns>
        public ModuleBuilder WithAliases(params string[] aliases)
        {
            _aliases.AddRange(aliases);
            return this;
        }

        /// <summary>
        /// Adds a submodule to the current module builder.
        /// </summary>
        /// <param name="builder">The module to add as a submodule</param>
        /// <returns>The current instnace, for chaining calls</returns>
        public ModuleBuilder AddSubmodule(ModuleBuilder builder)
        {
            builder.Parent = this;
            _submodules.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds an attribute to the created Module.
        /// </summary>
        /// <param name="attribute">The attribute to add</param>
        /// <returns>The current instance, for chaining calls</returns>
        public ModuleBuilder AddAttribute(Attribute attribute)
        {
            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds a command to the created Module.
        /// </summary>
        /// <param name="command">The command to add</param>
        /// <returns>The current instance, for chaining calls</returns>
        public ModuleBuilder AddCommand(CommandBuilder command)
        {
            command.Module = this;
            _commands.Add(command);
            return this;
        }

        /// <summary>
        /// Builds the module, including submodules and commands.
        /// </summary>
        /// <returns>The built module</returns>
        public ModuleInfo Build()
            => Build(null);

        internal ModuleInfo Build(ModuleInfo parent)
        {
            return new ModuleInfo(parent, Aliases, Attributes, Submodules, Commands);
        }
    }
}
