using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakeTests
{
    [TestClass]
    public class ComputeEngineTest
    {
        [TestMethod]
        public async Task PrimitiveToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"integer", "42" },
                    {"string", "\"s\"" }
                }
            };
            var inputs = ImmutableDictionary<string, string>.Empty.Add("url", "http://bing.com");
            var result = await CompileAndComputeAsync(description, inputs);

            Assert.AreEqual(0, result.Variables.Count, "Variables");
            Assert.AreEqual(2, result.Outputs.Count, "Outputs");
            Assert.AreEqual(int.Parse(description.Outputs["integer"]), result.Outputs["integer"], "Integer");
            Assert.AreEqual(description.Outputs["string"].Trim('"'), result.Outputs["string"], "string");
        }

        [TestMethod]
        public async Task InputToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "url" },
                }
            };
            var inputs = ImmutableDictionary<string, string>.Empty.Add("url", "http://bing.com");
            var result = await CompileAndComputeAsync(description, inputs);

            Assert.AreEqual(0, result.Variables.Count, "Variables");
            Assert.AreEqual(1, result.Outputs.Count, "Outputs");
            Assert.AreEqual(inputs.First().Value, result.Outputs.First().Value, "Value");
        }

        private static async Task<ComputeResult> CompileAndComputeAsync(
            FunctionDescription description,
            IImmutableDictionary<string, string> inputs)
        {
            var compiler = new Compiler();
            var compiled = await compiler.CompileAsync(description);
            var engine = new ComputeEngine(new Quotas());
            var outputs = await engine.ComputeAsync(inputs, compiled);

            return outputs;
        }
    }
}