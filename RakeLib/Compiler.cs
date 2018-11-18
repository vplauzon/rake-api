﻿using PasApiClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Compiler
    {
        private static readonly string _grammar = GetResource("Grammar.pas");
        private readonly ParserClient _parserClient;

        public Compiler() : this(ParserClient.CreateFromBaseUri(new Uri("http://pas-api.dev.vplauzon.com/")))
        {
        }

        public Compiler(ParserClient parserClient)
        {
            _parserClient = parserClient ?? throw new ArgumentNullException(nameof(parserClient));
        }

        public async Task<CompiledCompute> CompileExpressionAsync(string expression)
        {
            var result = await _parserClient.SingleParseAsync(_grammar, "expression", expression);

            if(result.IsMatch)
            {
                //result.RuleMatch.NamedChildren
                throw new NotImplementedException();
            }
            else
            {
                return null;
            }
        }

        public async Task<CompiledFunction> CompileFunctionAsync(FunctionDescription description)
        {
            await Task.CompletedTask;

            throw new NotImplementedException();
        }

        private static string GetResource(string resourceName)
        {
            var type = typeof(RakeLib.Compiler);
            var assembly = type.GetTypeInfo().Assembly;
            var fullResourceName = $"{type.Namespace}.{resourceName}";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }
    }
}