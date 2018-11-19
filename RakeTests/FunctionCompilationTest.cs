using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RakeTests
{
    [TestClass]
    public class FunctionCompilationTest
    {
        [TestMethod]
        public async Task SmokeTest()
        {
            var description = new FunctionDescription
            {
                ApiVersion = "1.0",
                Inputs = new[] { "url", "count" },
                Variables = new[]
                {
                    new Variable<string>
                    {
                        Name="intCount",
                        Description="count.parseInt()"
                    },
                    new Variable<string>
                    {
                        Name="content",
                        Description="url.fetchContent()"
                    }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" },
                    {"date", "content.xpath(\"date\")" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileFunctionAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(2, compiled.Inputs.Length, "Inputs");
            Assert.AreEqual(2, compiled.Variables.Length, "Variables");
            Assert.AreEqual(2, compiled.Outputs.Count, "Outputs");
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatInput()
        {
            var description = new FunctionDescription
            {
                ApiVersion = "1.0",
                Inputs = new[] { "url", "url" },
                Variables = new Variable<string>[0],
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileFunctionAsync(description);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatVariable()
        {
            var description = new FunctionDescription
            {
                ApiVersion = "1.0",
                Inputs = new[] { "url", "count" },
                Variables = new[]
                {
                    new Variable<string>
                    {
                        Name="intCount",
                        Description="count.parseInt()"
                    },
                    new Variable<string>
                    {
                        Name="intCount",
                        Description="url.fetchContent()"
                    }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileFunctionAsync(description);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task RepeatInputInVariable()
        {
            var description = new FunctionDescription
            {
                ApiVersion = "1.0",
                Inputs = new[] { "url", "count" },
                Variables = new[]
                {
                    new Variable<string>
                    {
                        Name="url",
                        Description="count.parseInt()"
                    }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileFunctionAsync(description);
        }

        [TestMethod]
        [ExpectedException(typeof(ComputeException))]
        public async Task InvalidVariableName()
        {
            var description = new FunctionDescription
            {
                ApiVersion = "1.0",
                Inputs = new[] { "url", "count" },
                Variables = new[]
                {
                    new Variable<string>
                    {
                        Name="$recount",
                        Description="count.parseInt()"
                    }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"content", "content.xpath(\"div\")" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileFunctionAsync(description);
        }
    }
}