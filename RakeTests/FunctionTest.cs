using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RakeTests
{
    [TestClass]
    public class FunctionTest
    {
        [TestMethod]
        public void SimpleXPath()
        {
            var content = GetResource("simple-web.html");
            var description = GetFunctionDescription("simple-xpath.yaml");
            var compiler = new FunctionCompiler();
        }

        private string GetResource(string resourceName)
        {
            var type = this.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            var fullResourceName = $"{type.Namespace}.Functions.{resourceName}";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }

        private FunctionDescription GetFunctionDescription(string resourceName)
        {
            var content = GetResource(resourceName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            var function = deserializer.Deserialize<FunctionDescription>(content);

            return function;
        }
    }
}