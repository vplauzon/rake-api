using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using System;
using System.Threading.Tasks;

namespace RakeTests
{
    [TestClass]
    public class ExpressionCompilationTest
    {
        #region References
        [TestMethod]
        public async Task Integer()
        {
            var compiler = new Compiler();
            var compute = await compiler.ParseExpressionAsync("42");

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
            var compute = await compiler.ParseExpressionAsync("\"my text\"");

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
            var compute = await compiler.ParseExpressionAsync("input");

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
            var compute = await compiler.ParseExpressionAsync("input.myproperty");

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
            var compute = await compiler.ParseExpressionAsync("input." + string.Join('.', chain));

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
            var compute = await compiler.ParseExpressionAsync("3.my_Method()");

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
            var compute = await compiler.ParseExpressionAsync("\"name\"." + string.Join("( ).", chain) + " (  )");

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

        #region Methods with parameters
        [TestMethod]
        public async Task MethodWithOneParameter()
        {
            var compiler = new Compiler();
            var compute = await compiler.ParseExpressionAsync("3.myMethod( 4  )");

            Assert.IsNotNull(compute);
            Assert.AreEqual(3, compute.Reference.Integer, "Integer");
            Assert.IsFalse(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual("myMethod", compute.MethodInvoke.Name, "Name");
            Assert.AreEqual(1, compute.MethodInvoke.Parameters.Length, "Parameters");
            Assert.AreEqual(4, compute.MethodInvoke.Parameters[0].Reference.Integer, "Parameters-0");
            Assert.IsNull(compute.MethodInvoke.Next, "Next");
        }

        [TestMethod]
        public async Task MethodWithTwoParameters()
        {
            var compiler = new Compiler();
            var compute = await compiler.ParseExpressionAsync("3.myMethod( 4 ,  \"hello\")");

            Assert.IsNotNull(compute);
            Assert.AreEqual(3, compute.Reference.Integer, "Integer");
            Assert.IsFalse(compute.MethodInvoke.IsProperty, "IsProperty");
            Assert.AreEqual("myMethod", compute.MethodInvoke.Name, "Name");
            Assert.AreEqual(2, compute.MethodInvoke.Parameters.Length, "Parameters");
            Assert.AreEqual(4, compute.MethodInvoke.Parameters[0].Reference.Integer, "Parameters-0");
            Assert.AreEqual("hello", compute.MethodInvoke.Parameters[1].Reference.QuotedString, "Parameters-1");
            Assert.IsNull(compute.MethodInvoke.Next, "Next");
        }
        #endregion

        [TestMethod]
        public async Task Mix()
        {
            var compiler = new Compiler();
            var compute = await compiler.ParseExpressionAsync("myvar.Count.Add( 42).Substract(22, none).isOk");

            Assert.IsNotNull(compute);
            Assert.AreEqual("myvar", compute.Reference.Identifier, "Identifier");
            Assert.IsTrue(compute.MethodInvoke.IsProperty, "IsProperty");

            Assert.AreEqual("Count", compute.MethodInvoke.Name, "Name");

            Assert.AreEqual("Add", compute.MethodInvoke.Next.Name, "Next.Name");
            Assert.AreEqual(1, compute.MethodInvoke.Next.Parameters.Length, "Next.Parameters.Length");
            Assert.AreEqual(
                42,
                compute.MethodInvoke.Next.Parameters[0].Reference.Integer,
                "Next.Parameters[0].Reference.Integer");

            Assert.AreEqual("Substract", compute.MethodInvoke.Next.Next.Name, "Next.Next.Name");
            Assert.AreEqual(
                2,
                compute.MethodInvoke.Next.Next.Parameters.Length,
                "Next.Next.Parameters.Length");
            Assert.AreEqual(
                22,
                compute.MethodInvoke.Next.Next.Parameters[0].Reference.Integer,
                "Next.Next.Parameters[0].Reference.Integer");
            Assert.AreEqual(
                "none",
                compute.MethodInvoke.Next.Next.Parameters[1].Reference.Identifier,
                "Next.Next.Parameters[1].Reference.Identifier");

            Assert.AreEqual("isOk", compute.MethodInvoke.Next.Next.Next.Name, "Next.Next.Next.Name");
            Assert.IsTrue(compute.MethodInvoke.Next.Next.Next.IsProperty, "Next.Next.Next.IsProperty");
        }
    }
}