using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace RakeTests
{
    [TestClass]
    public class FunctionTest
    {
        [TestMethod]
        public void SimpleXPath()
        {
            var w = GetResource("simple-web.html");
            var f = GetResource("simple-xpath.yaml");
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
    }
}