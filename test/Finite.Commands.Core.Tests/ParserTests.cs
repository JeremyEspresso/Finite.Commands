using System;
using System.Threading.Tasks;
using Xunit;

namespace Finite.Commands.Tests
{
    public class ParserTests
    {
        private static readonly DefaultCommandParser<TestContext> Parser
            = new DefaultCommandParser<TestContext>();
        private static readonly CommandService<TestContext> Service
             = new CommandServiceBuilder<TestContext>()
                .AddCommandParser<DefaultCommandParser<TestContext>>()
                .AddTypeReaderFactory<NullTypeReaderFactory>()
                .AddModule<ParserTestModule>()
                .BuildCommandService();

        [Fact]
        public void ValidParseWithNoParams()
        {
            Assert.True(Parse("no_params").IsSuccess);
            Assert.True(Parse("no_params but actually with params").IsSuccess);
        }

        [Fact]
        public void ValidParseWithOneRequiredParam()
        {
            Assert.False(Parse("required_param").IsSuccess);
            Assert.False(Parse("required_param a").IsSuccess);
            Assert.True(Parse("required_param 1").IsSuccess);
            Assert.True(Parse("required_param 1 with extra").IsSuccess);
        }

        [Fact]
        public void ValidParseWithMultipleRequiredParams()
        {
            Assert.False(Parse("multiple_required_params").IsSuccess);
            Assert.False(Parse("multiple_required_params a").IsSuccess);
            Assert.False(Parse("multiple_required_params 1").IsSuccess);
            Assert.False(Parse("multiple_required_params 1 a").IsSuccess);
            Assert.True(Parse("multiple_required_params 1 2").IsSuccess);
            Assert.True(Parse("multiple_required_params 1 2 and more").IsSuccess);
        }

        [Fact]
        public void ValidParseWithQuotedParams()
        {
            Assert.True(Parse("quoted_params abc 1").IsSuccess);
            Assert.False(Parse("quoted_params oh no 1").IsSuccess);
            Assert.True(Parse("quoted_params 'this works though' 1").IsSuccess);
        }

        [Fact]
        public void ValidParseWithAnArrayOfParams()
        {
            Assert.True(Parse("an_array_of_params").IsSuccess);
            Assert.False(Parse("an_array_of_params a").IsSuccess);
            Assert.True(Parse("an_array_of_params 1").IsSuccess);
            Assert.False(Parse("an_array_of_params 1 a").IsSuccess);
            Assert.True(Parse("an_array_of_params 1 2").IsSuccess);
        }

        [Fact]
        public void ValidParseWithOneOptionalParam()
        {
            Assert.True(Parse("optional_param").IsSuccess);
            Assert.False(Parse("optional_param a").IsSuccess);
            Assert.True(Parse("optional_param 1").IsSuccess);
            Assert.True(Parse("optional_param 1 a").IsSuccess);
        }

        [Fact]
        public void ValidParseWithMultipleOptionalParams()
        {
            Assert.True(Parse("multiple_optional_params").IsSuccess);
            Assert.False(Parse("multiple_optional_params a").IsSuccess);
            Assert.True(Parse("multiple_optional_params 1").IsSuccess);
            Assert.False(Parse("multiple_optional_params 1 a").IsSuccess);
            Assert.True(Parse("multiple_optional_params 1 2").IsSuccess);
            Assert.True(Parse("multiple_optional_params 1 2 a").IsSuccess);
        }

        [Fact]
        public void ValidParseWithRemainderParams()
        {
            Assert.False(Parse("remainder_param").IsSuccess);
            Assert.True(Parse("remainder_param a").IsSuccess);
            Assert.True(Parse("remainder_param 'oh okay this should work'").IsSuccess);
            Assert.True(Parse("remainder_param ok this should work too").IsSuccess);
        }

        [Fact]
        public void ValidParseWithRemainderOptionalParams()
        {
            Assert.True(Parse("remainder_optional_param").IsSuccess);
            Assert.True(Parse("remainder_optional_param a").IsSuccess);
            Assert.True(Parse("remainder_optional_param 'oh okay this should work'").IsSuccess);
            Assert.True(Parse("remainder_optional_param ok this should work too").IsSuccess);
        }

        private IResult Parse(string msg)
        {
            var context = new TestContext()
            {
                Message = msg
            };

            return Parser.Parse(
                new CommandExecutionContext(Service, context, null!));
        }

        public class ParserTestModule : ModuleBase<TestContext>
        {
            [Command("no_params")]
            public Task NoParams()
                => Task.CompletedTask;

            [Command("required_param")]
            public Task RequiredParam(int param)
                => Task.CompletedTask;

            [Command("multiple_required_params")]
            public Task MultipleRequiredParams(int param, long otherParam)
                => Task.CompletedTask;

            [Command("quoted_params")]
            public Task QuotedParams(string param1, int param2)
                => Task.CompletedTask;

            [Command("an_array_of_params")]
            public Task AnArrayOfParams(params int[] parameters)
                => Task.CompletedTask;

            [Command("optional_param")]
            public Task OptionalParam(int spooky = 5)
                => Task.CompletedTask;

            [Command("multiple_optional_params")]
            public Task MultipleOptionalParams(int really = 1337, long spooky = 5)
                => Task.CompletedTask;

            [Command("remainder_param")]
            public Task RemainderParam([Remainder]string message)
                => Task.CompletedTask;

            [Command("remainder_optional_param")]
            public Task RemainderOptionalParam([Remainder]string message = "nerds")
                => Task.CompletedTask;
        }
    }
}
