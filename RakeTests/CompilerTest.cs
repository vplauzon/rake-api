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
        public async Task InputOutput()
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
            Assert.AreEqual(2, compiled.Computes, "Computes");
        }
    }
}