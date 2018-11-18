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
        #region References
        [TestMethod]
        public async Task Integer()
        {
            var compiler = new Compiler();
            var compute = await compiler.CompileExpressionAsync("42");

            Assert.IsNotNull(compute);
            Assert.AreEqual(42, compute.Reference.Integer, "Integer");
            Assert.IsNull(compute.Reference.Identifier, "Identifier");
            Assert.IsNull(compute.Reference.QuotedString, "QuotedString");
            Assert.IsNull(compute.MethodInvoke, "MethodInvoke");
        }

        [TestMethod]
        public async Task QuotedString()
        {
            var compiler = new Compiler();
            var compute = await compiler.CompileExpressionAsync("\"my text\"");

            Assert.IsNotNull(compute);
            Assert.AreEqual("my text", compute.Reference.QuotedString, "QuotedString");
            Assert.IsNull(compute.Reference.Integer, "Integer");
            Assert.IsNull(compute.Reference.Identifier, "Identifier");
            Assert.IsNull(compute.MethodInvoke, "MethodInvoke");
        }

        [TestMethod]
        public async Task Identifier()
        {
            var compiler = new Compiler();
            var compute = await compiler.CompileExpressionAsync("input");

            Assert.IsNotNull(compute);
            Assert.AreEqual("input", compute.Reference.Identifier, "Identifier");
            Assert.IsNull(compute.Reference.Integer, "Integer");
            Assert.IsNull(compute.Reference.QuotedString, "QuotedString");
            Assert.IsNull(compute.MethodInvoke, "MethodInvoke");
        }
        #endregion

        [TestMethod]
        public async Task Property()
        {
            var compiler = new Compiler();
            var compute = await compiler.CompileExpressionAsync("input.myproperty");

            Assert.IsNotNull(compute);
            Assert.AreEqual("input", compute.Reference.Identifier, "Identifier");
            Assert.IsTrue(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual("myproperty", compute.MethodInvoke.Name, "Name");
            Assert.IsNull(compute.MethodInvoke.Parameters, "Parameters");
            Assert.IsNull(compute.MethodInvoke.Next, "Next");
        }
    }
}