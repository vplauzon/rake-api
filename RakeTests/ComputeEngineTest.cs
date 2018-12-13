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
        #region Inner types
        private static class StringHelper
        {
            public static int LengthProperty(string s)
            {
                return s.Length;
            }
        }
        #endregion

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

        [TestMethod]
        public async Task VariableToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>()
                {
                    {"tmp", "42" },
                },
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "tmp" },
                }
            };
            var inputs = ImmutableDictionary<string, string>.Empty.Add("url", "http://bing.com");
            var result = await CompileAndComputeAsync(description, inputs);

            Assert.AreEqual(1, result.Variables.Count, "Variables");
            Assert.AreEqual(1, result.Outputs.Count, "Outputs");
            Assert.AreEqual(int.Parse(description.Variables.First().Value), result.Variables.First().Value, "Var");
            Assert.AreEqual(int.Parse(description.Variables.First().Value), result.Outputs.First().Value, "Outputs");
        }

        [TestMethod]
        public async Task PredefinedVariableToOutput()
        {
            var description = new FunctionDescription
            {
                Inputs = new[] { "url" },
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", "truth" },
                }
            };
            var inputs = ImmutableDictionary<string, string>.Empty.Add("url", "http://bing.com");
            var predefinedVariables = ImmutableDictionary<string, object>.Empty.Add("truth", 42);
            var result = await CompileAndComputeAsync(description, inputs, predefinedVariables: predefinedVariables);

            Assert.AreEqual(0, result.Variables.Count, "Variables");
            Assert.AreEqual(1, result.Outputs.Count, "Outputs");
            Assert.AreEqual(predefinedVariables.First().Value, result.Outputs.First().Value, "Outputs");
        }

        [TestMethod]
        public async Task Property()
        {
            var word = "hello";
            var description = new FunctionDescription
            {
                Inputs = new string[0],
                Variables = new Dictionary<string, string>(),
                Outputs = new Dictionary<string, string>()
                {
                    {"outThere", $"\"{word}\".length" },
                }
            };
            var inputs = ImmutableDictionary<string, string>.Empty;
            var result = await CompileAndComputeAsync(description, inputs);

            Assert.AreEqual(0, result.Variables.Count, "Variables");
            Assert.AreEqual(1, result.Outputs.Count, "Outputs");
            Assert.AreEqual(word.Length, result.Outputs.First().Value, "Outputs");
        }

        private static async Task<ComputeResult> CompileAndComputeAsync(
            FunctionDescription description,
            IImmutableDictionary<string, string> inputs,
            MethodSet methodSet = null,
            IImmutableDictionary<string, object> predefinedVariables = null)
        {
            var compiler = new Compiler(predefinedVariables == null ? null : predefinedVariables.Keys);
            var compiled = await compiler.CompileAsync(description);
            var engine = new ComputeEngine(new Quotas(), methodSet, predefinedVariables);
            var outputs = await engine.ComputeAsync(inputs, compiled);

            return outputs;
        }
    }
}