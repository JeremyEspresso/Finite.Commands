using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finite.Commands
{
    /// <summary>
    /// The base interface for all command services.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        /// Gets a collection of modules which this command service can use
        /// </summary>
        IReadOnlyCollection<ModuleInfo> Modules { get; }

        /// <summary>
        /// Gets the factory used for creating type readers
        /// </summary>
        ITypeReaderFactory TypeReaderFactory { get; }

        /// <summary>
        /// Executes any stored pipelines on a context, returning any result
        /// they produce.
        /// </summary>
        /// <param name="context">
        /// The contextual message data to execute pipelines on.
        /// </param>
        /// <param name="services">
        /// A provider for services used to create modules based on their
        /// dependencies.
        /// </param>
        /// <returns>
        /// A <see cref="IResult"/> produced somewhere in the pipeline chain.
        /// </returns>
        Task<IResult> ExecuteAsync(ICommandContext context,
            IServiceProvider services);

        /// <summary>
        /// Finds commands matching the given tokenized message.
        /// </summary>
        /// <param name="tokenizerResult">
        /// The tokenized message to look for commands in.
        /// </param>
        /// <returns>
        /// Any commands matched by the <paramref name="tokenizerResult"/>.
        /// </returns>
        IEnumerable<CommandMatch> FindCommands(TokenizerResult tokenizerResult);
    }
}
