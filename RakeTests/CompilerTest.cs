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
    public class CompilerTest
    {
        [TestMethod]
        public async Task InputToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(1, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task InputToVariableInputToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>()
                {
                    {"copyUrl", "url" }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(1, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task InputToVariableToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>()
                {
                    {"copyUrl", "url" }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "copyUrl" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(2, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task InputToVariableTo2Outputs()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>()
                {
                    {"copyUrl", "url" }
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url" },
                    {"outThere2", "copyUrl" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(3, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task Property()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url.length" },
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(2, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task ChainedProperty()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url.length.hash" },
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(3, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task ChainedPropertiesWithReuse()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url.length.hash" },
                    {"outThere2", "url.length.hash2" }
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(4, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task EmptyParamMethodInvoke()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url.crunch()" },
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(2, compiled.Computes.Length, "Computes");
        }

        [TestMethod]
        public async Task OneParamMethodInvoke()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url.crunch(3)" },
                }
            };
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);

            Assert.IsNotNull(compiled);
            Assert.AreEqual(description.Inputs.Length, compiled.InputNames.Length, "Inputs");
            Assert.AreEqual(3, compiled.Computes.Length, "Computes");
        }
    }
}