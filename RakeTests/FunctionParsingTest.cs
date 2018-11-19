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
            var compiler = new Parser();
            var compiled = await compiler.CompileFunctionAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(2, compiled.Inputs.Length, "Inputs");
            Assert.AreEqual(2, compiled.Variables.Count, "Variables");
            Assert.AreEqual(2, compiled.Outputs.Count, "Outputs");
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
            var compiler = new Parser();
            var compiled = await compiler.CompileFunctionAsync(description);
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
            var compiler = new Parser();
            var compiled = await compiler.CompileFunctionAsync(description);
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
            var compiler = new Parser();
            var compiled = await compiler.CompileFunctionAsync(description);
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
            var compiler = new Parser();
            var compiled = await compiler.CompileFunctionAsync(description);
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
            var compiler = new Parser();
            var compiled = await compiler.CompileFunctionAsync(description);
        }
    }
}