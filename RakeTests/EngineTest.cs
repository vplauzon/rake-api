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
    public class EngineTest
    {
        [TestMethod]
        public async Task SimpleXPath()
        {
            var content = GetResourceAsync("simple-web.html");
            var function = await GetExecutableFunctionAsync("simple-xpath.yaml");
        }

        private async Task<string> GetResourceAsync(string resourceName)
        {
            var type = this.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            var fullResourceName = $"{type.Namespace}.Functions.{resourceName}";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = await reader.ReadToEndAsync();

                return text;
            }
        }

        private async Task<FunctionDescription> GetFunctionDescriptionAsync(string resourceName)
        {
            var content = await GetResourceAsync(resourceName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            var function = deserializer.Deserialize<FunctionDescription>(content);

            return function;
        }

        private async Task<ExecutableFunction> GetExecutableFunctionAsync(string resourceName)
        {
            var description = await GetFunctionDescriptionAsync(resourceName);
            var compiler = new Parser();
            var maker = new FunctionMaker();
            var compiled = await compiler.CompileFunctionAsync(description);
            var executable = maker.Make(compiled);

            return executable;
        }
    }
}