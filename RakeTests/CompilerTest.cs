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
            Assert.AreEqual(2, compiled.Computes.Length, "Computes");
        }
    }
}