using System;
using System.Linq;
using Xunit;

namespace Finite.Commands.Tests
{
    public class CommandMapTests
    {
        private static CommandInfo CreateCommand()
            => new CommandInfo(null, null, null, null, null,
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
            new string[]{"nested", "command"},
            new string[]{"nested"},
            new string[]{"nested", "command"},
            new string[]{"not", "a", "command"})]
        public void FindCommands(string[] command1PathStr,
            string[] command2PathStr, string[] searchPathStr,
            string[] invalidSearchPathStr)
        {
            var command1Path = command1PathStr
                .Select(x => x.AsMemory()).ToArray();
            var command2Path = command2PathStr
                .Select(x => x.AsMemory()).ToArray();
            var searchPath = searchPathStr
                .Select(x => x.AsMemory()).ToArray();
            var invalidSearchPath = invalidSearchPathStr
                .Select(x => x.AsMemory()).ToArray();
            var map = new CommandMap();
            var testCommand = CreateCommand();
            var testCommand2 = CreateCommand();

            Assert.True(map.AddCommand(command1Path, testCommand));
            Assert.True(map.AddCommand(command2Path, testCommand2));

            var commands = map.GetCommands(searchPath);
            Assert.NotNull(commands);
            var commandsArray = commands.ToArray();
            Assert.Equal(commandsArray.Length, 2);

            commands = map.GetCommands(invalidSearchPath);
            Assert.NotNull(commands);
            commandsArray = commands.ToArray();
            Assert.Empty(commandsArray);
        }

        [Theory]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "module",
            1)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "module ThisIsAnArgument",
            1)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "module stat",
            2)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "module stat ThisIsAnArgument",
            2)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "module stats",
            2)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "module stats ThisIsAnArgument",
            2)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
            "unrelated",
            0)]
        [InlineData(
            new string[]{"module", "module stat", "module stats"},
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

            var commands = map.GetCommands(
                searchQuery.Split(' ')
                    .Select(x => x.AsMemory()).ToArray());
            Assert.NotNull(commands);

            var commandsArray = commands.ToArray();
            Assert.Equal(expectedQueryResults, commandsArray.Length);
        }
    }
}
