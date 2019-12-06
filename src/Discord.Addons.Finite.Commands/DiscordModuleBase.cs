using Finite.Commands;

namespace Discord.Addons.Finite.Commands
{
    /// <summary>
    /// A <see cref="ModuleBase{TContext}" /> specialised for use with
    /// Discord.Net.
    /// </summary>
    public abstract class DiscordModuleBase : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Creates a <see cref="IResult"/> representing a message to respond
        /// with.
        /// </summary>
        /// <param name="body">
        /// The message body to respond with.
        /// </param>
        /// <returns>
        /// Returns a <see cref="MessageResult"/> with the given content.
        /// </returns>
        protected IResult Message(string body)
            => new MessageResult(content: body);
    }
}
