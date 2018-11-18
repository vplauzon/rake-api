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
            if (string.IsNullOrWhiteSpace(description.ApiVersion))
            {
                throw new ArgumentNullException(nameof(description.ApiVersion));
            }
            if (description.Inputs == null)
            {
                throw new ArgumentNullException(nameof(description.Inputs));
            }
            if (description.Variables == null)
            {
                throw new ArgumentNullException(nameof(description.Variables));
            }
            if (description.Outputs == null)
            {
                throw new ArgumentNullException(nameof(description.Outputs));
            }

            var variableDescriptions = from v in description.Variables
                                       select v.Description;
            var outputDescriptions = from o in description.Outputs
                                     select o.Value;
            var descriptions = variableDescriptions.Concat(outputDescriptions);
            var results = await _parserClient.MultipleParseAsync(_grammar, "expression", descriptions);
            var variableResults = results.Take(variableDescriptions.Count());
            var outputResults = results.Skip(variableDescriptions.Count());
            var namedVariableResults = description.Variables.Zip(
                variableResults,
                (v, r) => KeyValuePair.Create(v.Name, r));
            var namedOutputResults = description.Outputs.Zip(
                outputResults,
                (o, r) => KeyValuePair.Create(o.Key, r));

            ValidateCompilation(namedVariableResults, "Variable");
            ValidateCompilation(namedOutputResults, "Output");

            var variables = from v in namedVariableResults
                            select new Variable<CompiledCompute>
                            {
                                Name = v.Key,
                                Description = BuildExpression(v.Value.RuleMatch)
                            };
            var outputs = from o in namedOutputResults
                          select KeyValuePair.Create(
                              o.Key,
                              BuildExpression(o.Value.RuleMatch));

            return new CompiledFunction
            {
                ApiVersion = description.ApiVersion,
                Inputs = description.Inputs,
                Variables = variables.ToArray(),
                Outputs = new Dictionary<string, CompiledCompute>(outputs)
            };
        }

        private void ValidateCompilation(
            IEnumerable<KeyValuePair<string, ParsingResult>> namedResults,
            string resultType)
        {
            foreach (var r in namedResults)
            {
                var name = r.Key;
                var result = r.Value;

                if (!result.IsMatch)
                {
                    throw new ComputeException($"{resultType} '{name}' can't be compiled");
                }
            }
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
                var parameterList = genericParameterList.NamedChildren["paramList"];
                var head = parameterList.NamedChildren["head"];
                var tail = parameterList.NamedChildren["tail"];
                var headExpression = BuildExpression(head);

                if (tail.Children != null)
                {
                    var tailExpressions = from match in tail.Children
                                          select BuildExpression(match.NamedChildren["e"]);
                    var parameters = tailExpressions.Prepend(headExpression).ToArray();

                    return parameters;
                }
                else
                {
                    return new[] { headExpression };
                }
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