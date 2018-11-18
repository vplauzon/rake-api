using PasApiClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Compiler
    {
        private static readonly string _grammar = GetResource("Grammar.pas");
        private static readonly CompiledCompute[] _emptyComputeArray = new CompiledCompute[0];

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

            if (result.IsMatch)
            {
                return BuildExpression(result.RuleMatch);
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

        private CompiledCompute BuildExpression(RuleMatchResult ruleMatch)
        {
            var reference = BuildReference(ruleMatch.NamedChildren["ref"]);
            var methodChildren = ruleMatch.NamedChildren["method"].Children;

            if (methodChildren != null)
            {
                var methodInvoke = BuildMethodInvoke(methodChildren);

                return new CompiledCompute
                {
                    Reference = reference,
                    MethodInvoke = methodInvoke
                };
            }
            else
            {
                return new CompiledCompute { Reference = reference };
            }
        }

        private CompiledMethodInvoke BuildMethodInvoke(IEnumerable<RuleMatchResult> invokeList)
        {
            if (invokeList.Any())
            {
                var genericMethodInvoke = invokeList.First();
                var name = genericMethodInvoke.NamedChildren["name"].Text;
                var parameters = genericMethodInvoke.NamedChildren["params"];

                if (parameters.Children != null)
                {
                    var genericParameterList = parameters.Children.First();

                    return new CompiledMethodInvoke
                    {
                        IsProperty = false,
                        Name = name,
                        Parameters = BuildParameters(genericParameterList),
                        //  Recursion
                        Next = BuildMethodInvoke(invokeList.Skip(1))
                    };
                }
                else
                {
                    return new CompiledMethodInvoke
                    {
                        IsProperty = true,
                        Name = name,
                        //  Recursion
                        Next = BuildMethodInvoke(invokeList.Skip(1))
                    };
                }
            }
            else
            {   //  End of recursion
                return null;
            }
        }

        private CompiledCompute[] BuildParameters(RuleMatchResult genericParameterList)
        {
            if (genericParameterList.NamedChildren.Keys.First() == "empty")
            {
                return _emptyComputeArray;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private CompiledReference BuildReference(RuleMatchResult ruleMatch)
        {
            var child = ruleMatch.NamedChildren.First();
            var refType = child.Key;
            var reference = child.Value;

            switch (refType)
            {
                case "int":
                    return new CompiledReference { Integer = int.Parse(reference.Text) };
                case "string":
                    return new CompiledReference { QuotedString = reference.NamedChildren["s"].Text };
                case "id":
                    return new CompiledReference { Identifier = reference.Text };
                default:
                    throw new NotSupportedException($"Reference '{refType}'");
            }
        }
    }
}