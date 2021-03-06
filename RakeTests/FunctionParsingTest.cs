using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using RakeLib.Parsing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RakeTests
{
    [TestClass]
    public class FunctionParsingTest
    {
        [TestMethod]
        public async Task SmokeTest()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url", "count" },
                Variables = new Dictionary<string, string>()
                {
                    {"intCount","count.parseInt()"},
                    {"content","url.fetchContent()"}
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"formatted", "content.xpath(\"div\")" },
                    {"date", "content.xpath(\"date\")" }
                }
            };
            var parser = new Parser();
            var parsed = await parser.ParseFunctionAsync(description);

            AssertSameDimensions(description, parsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatInput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url", "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var parser = new Parser();
            var parsed = await parser.ParseFunctionAsync(description);

            AssertSameDimensions(description, parsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatInputInVariable()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url", "count" },
                Variables = new Dictionary<string, string>()
                {
                    {"url","count.parseInt()"}
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var parser = new Parser();
            var parsed = await parser.ParseFunctionAsync(description);

            AssertSameDimensions(description, parsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatVariableInOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url", "count" },
                Variables = new Dictionary<string, string>()
                {
                    {"a","count.parseInt()"}
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"a", "content.xpath(\"div\")" }
                }
            };
            var parser = new Parser();
            var parsed = await parser.ParseFunctionAsync(description);

            AssertSameDimensions(description, parsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatInputInOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url", "count" },
                Variables = new Dictionary<string, string>()
                {
                    {"a","count.parseInt()"}
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"url", "content.xpath(\"div\")" }
                }
            };
            var parser = new Parser();
            var parsed = await parser.ParseFunctionAsync(description);

            AssertSameDimensions(description, parsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task InvalidVariableName()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url", "count" },
                Variables = new Dictionary<string, string>()
                {
                    {"$recount","count.parseInt()"}
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var parser = new Parser();
            var parsed = await parser.ParseFunctionAsync(description);

            AssertSameDimensions(description, parsed);
        }

        private static void AssertSameDimensions(FunctionDescription description, ParsedFunction parsed)
        {
            Assert.IsNotNull(parsed);
            Assert.AreEqual(description.Inputs.Length, parsed.Inputs.Length, "Inputs");
            Assert.AreEqual(description.Variables.Count, parsed.Variables.Count, "Variables");
            Assert.AreEqual(description.Outputs.Count, parsed.Outputs.Count, "Outputs");
        }
    }
}