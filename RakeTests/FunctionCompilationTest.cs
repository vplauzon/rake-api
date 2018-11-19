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
                ApiVersion = "1",
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
    }
}