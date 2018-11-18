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

        #region Properties
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

        [TestMethod]
        public async Task PropertyChain()
        {
            var compiler = new Compiler();
            var chain = new[] { "a", "b", "c", "d" };
            var compute = await compiler.CompileExpressionAsync("input." + string.Join('.', chain));

            Assert.IsNotNull(compute);
            Assert.AreEqual("input", compute.Reference.Identifier, "Identifier");
            Assert.IsTrue(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual(chain[0], compute.MethodInvoke.Name, "Name");
            Assert.IsNull(compute.MethodInvoke.Parameters, "Parameters");
            Assert.AreEqual(chain[1], compute.MethodInvoke.Next.Name, "Next");
            Assert.AreEqual(chain[2], compute.MethodInvoke.Next.Next.Name, "Next.Next");
            Assert.AreEqual(chain[3], compute.MethodInvoke.Next.Next.Next.Name, "Next.Next.Next");
        }
        #endregion

        #region Empty Methods
        [TestMethod]
        public async Task EmptyMethod()
        {
            var compiler = new Compiler();
            var compute = await compiler.CompileExpressionAsync("3.my_Method()");

            Assert.IsNotNull(compute);
            Assert.AreEqual(3, compute.Reference.Integer, "Integer");
            Assert.IsFalse(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual("my_Method", compute.MethodInvoke.Name, "Name");
            Assert.AreEqual(0, compute.MethodInvoke.Parameters.Length, "Parameters");
            Assert.IsNull(compute.MethodInvoke.Next, "Next");
        }

        [TestMethod]
        public async Task EmptyMethodChain()
        {
            var compiler = new Compiler();
            var chain = new[] { "mi_ne", "Yours", "his", "hers" };
            var compute = await compiler.CompileExpressionAsync("\"name\"." + string.Join("( ).", chain)+" (  )");

            Assert.IsNotNull(compute);
            Assert.AreEqual("name", compute.Reference.QuotedString, "QuotedString");
            Assert.IsFalse(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual(chain[0], compute.MethodInvoke.Name, "Name");
            Assert.AreEqual(0, compute.MethodInvoke.Parameters.Length, "Parameters");
            Assert.AreEqual(chain[1], compute.MethodInvoke.Next.Name, "Next");
            Assert.AreEqual(chain[2], compute.MethodInvoke.Next.Next.Name, "Next.Next");
            Assert.AreEqual(chain[3], compute.MethodInvoke.Next.Next.Next.Name, "Next.Next.Next");
        }
        #endregion
    }
}