using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RakeTests
{
    [TestClass]
    public class CompilationTest
    {
        [TestMethod]
        public async Task SimpleXPath()
        {
            var compiler = new Compiler();
            var compute = await compiler.CompileExpressionAsync("input.myproperty");

            Assert.IsNotNull(compute);
            Assert.AreEqual("input", compute.Identifier, "Identifier");
            Assert.IsTrue(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual("myproperty", compute.MethodInvoke.Name, "Name");
            Assert.IsNull(compute.MethodInvoke.Parameters, "Parameters");
        }
    }
}