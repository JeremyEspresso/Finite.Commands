using System;
using System.Linq;
using Xunit;

namespace Finite.Commands.Tests
{
    public class CommandMapTests
    {
        private static CommandInfo CreateCommand()
            => new CommandInfo(null, null!, null!, null!, null!,
                Array.Empty<ParameterBuilder>());

        [Theory]
        [InlineData("nonNested")]
        [InlineData("nested", "command")]
        [InlineData("a", "third", "level")]
        public void AddRemove(params string[] pathStr)
        {
            var path = pathStr.Select(x => x.AsMemory()).ToArray();
            var map = new CommandMap();
            var testCommand = CreateCommand();
            var testCommand2 = CreateCommand();

            Assert.True(map.AddCommand(path, testCommand));
            Assert.True(map.AddCommand(path, testCommand2));
            Assert.True(map.RemoveCommand(path, testCommand));
            Assert.True(map.RemoveCommand(path, testCommand2));

            Assert.True(map.AddCommand(path, testCommand2));
            Assert.True(map.AddCommand(path, testCommand));
            Assert.True(map.RemoveCommand(path, testCommand2));
            Assert.True(map.RemoveCommand(path, testCommand));
        }

        [Theory]
        [InlineData(
            "nested command",
            "nested",
            "nested command",
            "not a command")]
        public void FindCommands(string command1PathStr,
            string command2PathStr, string searchPathStr,
            string invalidSearchPathStr)
        {
            var command1Path = command1PathStr.Split(' ')
                .Select(x => x.AsMemory()).ToArray();
            var command2Path = command2PathStr.Split(' ')
                .Select(x => x.AsMemory()).ToArray();
            var searchPath = searchPathStr.Split(' ')
                .Select(x => x.AsMemory()).ToArray();
            var invalidSearchPath = invalidSearchPathStr.Split(' ')
                .Select(x => x.AsMemory()).ToArray();
            var map = new CommandMap();
            var testCommand = CreateCommand();
            var testCommand2 = CreateCommand();

            Assert.True(map.AddCommand(command1Path, testCommand));
            Assert.True(map.AddCommand(command2Path, testCommand2));

            // TODO: rewrite this to not rely on the parser
            var parser = new DefaultCommandParser<TestContext>();
            var tokenizedSearchPath = parser.Tokenize(searchPathStr, 0);
            var tokenizedInvalidPath = parser.Tokenize(invalidSearchPathStr, 0);

            var commands = map.GetCommands(tokenizedSearchPath);
            Assert.NotNull(commands);
            var commandsArray = commands.ToArray();
            Assert.Equal(2, commandsArray.Length);

            commands = map.GetCommands(tokenizedInvalidPath);
            Assert.NotNull(commands);
            commandsArray = commands.ToArray();
            Assert.Empty(commandsArray);
        }

        [Theory]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "module",
            1)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "module ThisIsAnArgument",
            1)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "module stat",
            2)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "module stat ThisIsAnArgument",
            2)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "module stats",
            2)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "module stats ThisIsAnArgument",
            2)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "unrelated",
            0)]
        [InlineData(
            new string[] { "module", "module stat", "module stats" },
            "",
            0)]
        public void FindCommandsDefaultAlias(string[] aliases,
            string searchQuery, int expectedQueryResults)
        {
            var map = new CommandMap();
            var testCommand = CreateCommand();

            foreach (var alias in aliases)
            {
                var path = alias.Split(' ')
                    .Select(x => x.AsMemory()).ToArray();

                Assert.True(map.AddCommand(path, testCommand));
            }

            // TODO: rewrite this to not rely on the parser
            var parser = new DefaultCommandParser<TestContext>();
            var tokenized = parser.Tokenize(searchQuery, 0);

            var commands = map.GetCommands(tokenized);
            Assert.NotNull(commands);

            var commandsArray = commands.ToArray();
            Assert.Equal(expectedQueryResults, commandsArray.Length);
        }
    }
}
