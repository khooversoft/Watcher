using FluentAssertions;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Linq;
using WatcherCmd.Application;
using Xunit;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace WatcherCmd.Test
{
    public class OptionTests
    {
        private static readonly string[] _storeArguments = new[]
        {
            "Store:ConnectionString={connectionString}",
            "Store:AccountKey={AccountKey}",
            "Store:DatabaseName={DatabaseName}",
        };

        [Fact]
        public void GivenNoOption_ShouldReturnHelp()
        {
            IOption option = new OptionBuilder()
                .Build();

            option.Help.Should().BeTrue();
        }

        [Fact]
        public void GivenHelpOption_ShouldReturnHel()
        {
            IOption option = new OptionBuilder()
                .SetArgs(new[] { "help" })
                .Build();

            option.Help.Should().BeTrue();
        }

        [Fact]
        public void GivenBadOption_ShouldThrow()
        {
            Action act = () => new OptionBuilder()
                .SetArgs(new[] { "bad" })
                .Build();

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenImportWithFile_ShouldNotThrow()
        {
            Action act = () => new OptionBuilder()
                .SetArgs(new[]
                {
                    "Import",
                    "File={File}",
                }
                .Concat(_storeArguments)
                .ToArray())
                .Build();

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenImportWithoutFile_ShouldThrow()
        {
            Action act = () => new OptionBuilder()
                .SetArgs(new[]
                {
                    "Import",
                }
                .Concat(_storeArguments)
                .ToArray())
                .Build();

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("Agent", "Template")]
        [InlineData("Target", "Template")]
        public void GivenCommandThatRequireFile_ShouldNotThrow(string command, string action)
        {
            string file = Path.GetTempFileName();

            try
            {
                IOption option = new OptionBuilder()
                    .SetArgs(new[]
                    {
                        command,
                        action,
                        $"File={file}"
                    }
                    .Concat(_storeArguments)
                    .ToArray())
                    .Build();

                option.File.Should().Be(file);

                GetValueFromObject<bool>(option, command).Should().BeTrue();
                GetValueFromObject<bool>(option, action).Should().BeTrue();
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Theory]
        [InlineData("Agent", "Delete")]
        [InlineData("Target", "Delete")]
        public void GivenCommandThatRequireID_ShouldNotThrow(string command, string action)
        {
            IOption option = new OptionBuilder()
                .SetArgs(new[]
                {
                    command,
                    action,
                    "Id={id}"
                }
                .Concat(_storeArguments)
                .ToArray())
                .Build();

            option.Id.Should().Be("{id}");

            GetValueFromObject<bool>(option, command).Should().BeTrue();
            GetValueFromObject<bool>(option, action).Should().BeTrue();
        }

        [Theory]
        [InlineData("Store:ConnectionString")]
        [InlineData("Store:AccountKey")]
        [InlineData("Store:DatabaseName")]
        public void GivenInvalidStore_ShouldThrow(string command)
        {
            Action act = () => new OptionBuilder()
                .SetArgs(new[]
                {
                    "Agent",
                    "Delete",
                    "Id={id}"
                }
                .Concat(_storeArguments)
                .Where(x => !x.StartsWith(command))
                .ToArray())
                .Build();

            act.Should().Throw<ArgumentException>();
        }

        private T GetValueFromObject<T>(IOption value, string propertyName) => value.GetType()
            .GetProperty(propertyName)
            .VerifyNotNull($"Property {propertyName} does not exist")
            .GetValue(value)
            .VerifyNotNull("Property value is null")
            .Func(x => (T)x);
    }
}
