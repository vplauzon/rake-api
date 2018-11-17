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
            var function = await GetCompiledFunctionAsync("simple-xpath.yaml");
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

        private async Task<ExecutableFunction> GetCompiledFunctionAsync(string resourceName)
        {
            var description = await GetFunctionDescriptionAsync(resourceName);
            var compiler = new FunctionCompiler();
            var compiled = await compiler.CompileAsync(description);

            return compiled;
        }
    }
}