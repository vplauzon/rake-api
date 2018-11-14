using Microsoft.VisualStudio.TestTools.UnitTesting;
using RakeLib;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;

namespace RakeTests
{
    [TestClass]
    public class FunctionTest
    {
        [TestMethod]
        public void SimpleXPath()
        {
            var content = GetResource("simple-web.html");
            var function = GetFunctionDescription("simple-xpath.yaml");
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
            //DeserializerBuilder
            var deserializer = new Deserializer();
            var function = deserializer.Deserialize<FunctionDescription>(content);

            return function;
        }
    }
}