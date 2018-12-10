using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using RakeLib.Parsing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RakeTests
{
    [TestClass]
    public class ExpressionParsingTest
    {
        #region References
        [TestMethod]
        public async Task Integer()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("42");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.Primitive, "Primitive");
            Assert.AreEqual(42, expression.Primitive.Integer, "Integer");
            Assert.IsNull(expression.Primitive.Identifier, "Identifier");
            Assert.IsNull(expression.Primitive.QuotedString, "QuotedString");
            Assert.IsNull(expression.Property, "Property");
            Assert.IsNull(expression.MethodInvoke, "MethodInvoke");
        }

        [TestMethod]
        public async Task QuotedString()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("\"my text\"");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.Primitive, "Primitive");
            Assert.AreEqual("my text", expression.Primitive.QuotedString, "QuotedString");
            Assert.IsNull(expression.Primitive.Integer, "Integer");
            Assert.IsNull(expression.Primitive.Identifier, "Identifier");
            Assert.IsNull(expression.Property, "Property");
            Assert.IsNull(expression.MethodInvoke, "MethodInvoke");
        }

        [TestMethod]
        public async Task Identifier()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("input");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.Primitive, "Primitive");
            Assert.AreEqual("input", expression.Primitive.Identifier, "Identifier");
            Assert.IsNull(expression.Primitive.Integer, "Integer");
            Assert.IsNull(expression.Primitive.QuotedString, "QuotedString");
            Assert.IsNull(expression.Property, "Property");
            Assert.IsNull(expression.MethodInvoke, "MethodInvoke");
        }
        #endregion

        #region Properties
        [TestMethod]
        public async Task Property()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("input.myproperty");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.Property, "Property");
            Assert.AreEqual("input", expression.Property.Object.Primitive.Identifier, "Identifier");
            Assert.AreEqual("myproperty", expression.Property.Name, "Name");
            Assert.IsNull(expression.Primitive, "Primitive");
            Assert.IsNull(expression.MethodInvoke, "MethodInvoke");
        }

        [TestMethod]
        public async Task PropertyChain()
        {
            var compiler = new Parser();
            var chain = new[] { "a", "b", "c", "d" };
            var expression = await compiler.ParseExpressionAsync("input." + string.Join('.', chain));

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.Property, "Property");

            var names = new[]
            {
                expression.Property.Object.Property.Object.Property.Object.Property.Name,
                expression.Property.Object.Property.Object.Property.Name,
                expression.Property.Object.Property.Name,
                expression.Property.Name
            };

            Assert.AreEqual(
                "input",
                expression.Property.Object.Property.Object.Property.Object.Property.Object.Primitive.Identifier,
                "Identifier");

            Assert.IsTrue(Enumerable.SequenceEqual(chain, names), "Names");
            Assert.IsNull(expression.Primitive, "Primitive");
            Assert.IsNull(expression.MethodInvoke, "MethodInvoke");
        }
        #endregion

        #region Empty Methods
        [TestMethod]
        public async Task EmptyMethod()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("3.my_Method()");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.MethodInvoke, "MethodInvoke");
            Assert.AreEqual(3, expression.MethodInvoke.Object.Primitive.Integer, "Integer");
            Assert.AreEqual("my_Method", expression.MethodInvoke.Name, "Name");
            Assert.AreEqual(0, expression.MethodInvoke.Parameters.Length, "Parameters");
            Assert.IsNull(expression.Primitive, "Primitive");
            Assert.IsNull(expression.Property, "Property");
        }

        [TestMethod]
        public async Task EmptyMethodChain()
        {
            var compiler = new Parser();
            var chain = new[] { "mi_ne", "Yours", "his", "hers" };
            var expression = await compiler.ParseExpressionAsync("\"name\"." + string.Join("( ).", chain) + " (  )");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.MethodInvoke, "MethodInvoke");
            var names = new[]
            {
                expression.MethodInvoke.Object.MethodInvoke.Object.MethodInvoke.Object.MethodInvoke.Name,
                expression.MethodInvoke.Object.MethodInvoke.Object.MethodInvoke.Name,
                expression.MethodInvoke.Object.MethodInvoke.Name,
                expression.MethodInvoke.Name
            };
            Assert.AreEqual(
                "name",
                expression.MethodInvoke.Object.MethodInvoke.Object.MethodInvoke.Object.MethodInvoke.Object.Primitive.QuotedString,
                "QuotedString");
            Assert.IsNull(expression.Primitive, "Primitive");
            Assert.IsNull(expression.Property, "Property");
        }
        #endregion

        #region Methods with parameters
        [TestMethod]
        public async Task MethodWithOneParameter()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("3.myMethod( 4  )");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.MethodInvoke, "MethodInvoke");
            Assert.AreEqual(3, expression.MethodInvoke.Object.Primitive.Integer, "Integer");
            Assert.AreEqual("myMethod", expression.MethodInvoke.Name, "Name");
            Assert.AreEqual(1, expression.MethodInvoke.Parameters.Length, "Parameters");
            Assert.AreEqual(4, expression.MethodInvoke.Parameters[0].Primitive.Integer, "Parameters-0");
            Assert.IsNull(expression.Primitive, "Primitive");
            Assert.IsNull(expression.Property, "Property");
        }

        [TestMethod]
        public async Task MethodWithTwoParameters()
        {
            var compiler = new Parser();
            var expression = await compiler.ParseExpressionAsync("3.myMethod( 4 ,  \"hello\")");

            Assert.IsNotNull(expression, "Expression");
            Assert.IsNotNull(expression.MethodInvoke, "MethodInvoke");
            Assert.AreEqual(3, expression.MethodInvoke.Object.Primitive.Integer, "Integer");
            Assert.AreEqual("myMethod", expression.MethodInvoke.Name, "Name");
            Assert.AreEqual(2, expression.MethodInvoke.Parameters.Length, "Parameters");
            Assert.AreEqual(4, expression.MethodInvoke.Parameters[0].Primitive.Integer, "Parameters-0");
            Assert.AreEqual("hello", expression.MethodInvoke.Parameters[1].Primitive.QuotedString, "Parameters-1");
            Assert.IsNull(expression.Primitive, "Primitive");
            Assert.IsNull(expression.Property, "Property");
        }
        #endregion
    }
}