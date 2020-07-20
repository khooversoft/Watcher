using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Test
{
    [TestClass]
    public class StringVectorTests
    {
        [TestMethod]
        [DataRow("first", new string[] { "first" }, true)]
        [DataRow(@"first\second", new string[] { @"first\second" }, true)]
        [DataRow("first/second", new string[] { "first", "second" }, true)]
        [DataRow("first/second/third", new string[] { "first", "second", "third" }, true)]
        [DataRow("first/second/third", new string[] { "first", "second", "third" }, true)]
        public void StringVectorPrimaryTest(string value, string[] vectors, bool test)
        {
            StringVector sv = StringVector.Parse(value);

            sv.Count.Should().Be(vectors.Length);
            sv.Count.Should().Be(vectors.Length);
            sv.Where((x, i) => vectors[i] == x).Count().Should().Be(vectors.Length);

            sv.ToString().Should().Be(value ?? string.Empty);
        }

        [TestMethod]
        [DataRow(null, new string[0], true)]
        [DataRow("", new string[0], true)]
        public void StringVectorFailureTest(string value, string[] vectors, bool test)
        {
            StringVector sv = StringVector.Parse(value);

            sv.Count.Should().Be(vectors.Length);
            sv.Count.Should().Be(vectors.Length);
            sv.Where((x, i) => vectors[i] == x).Count().Should().Be(0);

            sv.ToString().Should().Be(value ?? string.Empty);
        }
    }
}
