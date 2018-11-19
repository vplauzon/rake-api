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
        private static readonly ParsedCompute[] _emptyComputeArray = new ParsedCompute[0];

        private readonly ParserClient _parserClient;

        public Compiler() : this(ParserClient.CreateFromBaseUri(new Uri("http://pas-api.dev.vplauzon.com/")))
        {
        }

        public Compiler(ParserClient parserClient)
        {
            _parserClient = parserClient ?? throw new ArgumentNullException(nameof(parserClient));
        }

        public async Task<ParsedCompute> ParseExpressionAsync(string expression)
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
            ValidateFunctionDescription(description);

            //  Bundle variable and output descriptions in one batch
            var variableDescriptions = from v in description.Variables
                                       select v.Value;
            var outputDescriptions = from o in description.Outputs
                                     select o.Value;
            var descriptions = variableDescriptions.Concat(outputDescriptions);
            var results = await _parserClient.MultipleParseAsync(_grammar, "expression", descriptions);
            //  Separate results into variables and outputs
            var variableResults = results.Take(variableDescriptions.Count());
            var outputResults = results.Skip(variableDescriptions.Count());
            var namedVariableResults = description.Variables.Zip(
                variableResults,
                (v, r) => KeyValuePair.Create(v.Key, r));
            var namedOutputResults = description.Outputs.Zip(
                outputResults,
                (o, r) => KeyValuePair.Create(o.Key, r));

            ValidateParsingMatch(namedVariableResults, "Variable");
            ValidateParsingMatch(namedOutputResults, "Output");

            var variables = from v in namedVariableResults
                            select KeyValuePair.Create(
                                v.Key,
                                BuildExpression(v.Value.RuleMatch));
            var outputs = from o in namedOutputResults
                          select KeyValuePair.Create(
                              o.Key,
                              BuildExpression(o.Value.RuleMatch));

            return new CompiledFunction
            {
                ApiVersion = description.ApiVersion,
                Inputs = description.Inputs,
                Variables = new Dictionary<string, ParsedCompute>(variables),
                Outputs = new Dictionary<string, ParsedCompute>(outputs)
            };
        }

        private void ValidateFunctionDescription(FunctionDescription description)
        {
            Func<string, bool> nameValidator = name => name.All(c => (c >= '0' && c <= '9')
            || (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || c == '_');

            //  Checking for nulls
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }
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

            //  Checking API Version value
            if (description.ApiVersion != ApiVersions.V10)
            {
                throw new ComputeException($"Version {description.ApiVersion} isn't supported");
            }

            //  Checking inputs
            if (description.Inputs.Any(i => string.IsNullOrWhiteSpace(i)))
            {
                throw new ComputeException("Inputs can't be blank");
            }
            if (description.Inputs.Any(i => !nameValidator(i)))
            {
                throw new ComputeException("Field names can only have alphanumeric characters and underscores");
            }
            var repeatedInput = (from g in description.Inputs.GroupBy(i => i)
                                 where g.Count() > 1
                                 select g.Key).FirstOrDefault();

            if (repeatedInput != null)
            {
                throw new ComputeException($"Input '{repeatedInput}' is repeated");
            }

            //  Checking variables
            if (description.Variables.Keys.Any(v => string.IsNullOrWhiteSpace(v)))
            {
                throw new ComputeException("Variable names can't be blank");
            }
            if (description.Variables.Values.Any(v => string.IsNullOrWhiteSpace(v)))
            {
                throw new ComputeException("Variable values can't be blank");
            }
            if (description.Variables.Keys.Any(i => !nameValidator(i)))
            {
                throw new ComputeException("Field names can only have alphanumeric characters and underscores");
            }

            var repeatedInputInVariable =
                description.Inputs.Intersect(description.Variables.Keys).FirstOrDefault();

            if (repeatedInputInVariable != null)
            {
                throw new ComputeException($"Variable '{repeatedInputInVariable}' has the same name as an input");
            }

            //  Checking outputs
            if (description.Outputs.Keys.Any(v => string.IsNullOrWhiteSpace(v)))
            {
                throw new ComputeException("Output names can't be blank");
            }
            if (description.Outputs.Values.Any(v => string.IsNullOrWhiteSpace(v)))
            {
                throw new ComputeException("Output values can't be blank");
            }
            if (description.Outputs.Any(i => !nameValidator(i.Key)))
            {
                throw new ComputeException("Field names can only have alphanumeric characters and underscores");
            }
        }

        private void ValidateParsingMatch(
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

        private ParsedCompute BuildExpression(RuleMatchResult ruleMatch)
        {
            var reference = BuildReference(ruleMatch.NamedChildren["ref"]);
            var methodChildren = ruleMatch.NamedChildren["method"].Children;

            if (methodChildren != null)
            {
                var methodInvoke = BuildMethodInvoke(methodChildren);

                return new ParsedCompute
                {
                    Reference = reference,
                    MethodInvoke = methodInvoke
                };
            }
            else
            {
                return new ParsedCompute { Reference = reference };
            }
        }

        private ParsedMethodInvoke BuildMethodInvoke(IEnumerable<RuleMatchResult> invokeList)
        {
            if (invokeList.Any())
            {
                var genericMethodInvoke = invokeList.First();
                var name = genericMethodInvoke.NamedChildren["name"].Text;
                var parameters = genericMethodInvoke.NamedChildren["params"];

                if (parameters.Children != null)
                {
                    var genericParameterList = parameters.Children.First();

                    return new ParsedMethodInvoke
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
                    return new ParsedMethodInvoke
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

        private ParsedCompute[] BuildParameters(RuleMatchResult genericParameterList)
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

        private ParsedReference BuildReference(RuleMatchResult ruleMatch)
        {
            var child = ruleMatch.NamedChildren.First();
            var refType = child.Key;
            var reference = child.Value;

            switch (refType)
            {
                case "int":
                    return new ParsedReference { Integer = int.Parse(reference.Text) };
                case "string":
                    return new ParsedReference { QuotedString = reference.NamedChildren["s"].Text };
                case "id":
                    return new ParsedReference { Identifier = reference.Text };
                default:
                    throw new NotSupportedException($"Reference '{refType}'");
            }
        }
    }
}