using FluentAssertions;
using System.Linq;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test
{
    public class StringVectorTests
    {
        [Theory]
        [InlineData("first", new string[] { "first" })]
        [InlineData(@"first\second", new string[] { @"first\second" })]
        [InlineData("first/second", new string[] { "first", "second" })]
        [InlineData("first/second/third", new string[] { "first", "second", "third" })]
        public void StringVectorPrimaryTest(string value, string[] vectors)
        {
            StringVector sv = StringVector.Parse(value);

            sv.Count.Should().Be(vectors.Length);
            sv.Count.Should().Be(vectors.Length);
            sv.Where((x, i) => vectors[i] == x).Count().Should().Be(vectors.Length);

            sv.ToString().Should().Be(value ?? string.Empty);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData("", new string[0])]
        public void StringVectorFailureTest(string value, string[] vectors)
        {
            StringVector sv = StringVector.Parse(value);

            sv.Count.Should().Be(vectors.Length);
            sv.Count.Should().Be(vectors.Length);
            sv.Where((x, i) => vectors[i] == x).Count().Should().Be(0);

            sv.ToString().Should().Be(value ?? string.Empty);
        }
    }
}
