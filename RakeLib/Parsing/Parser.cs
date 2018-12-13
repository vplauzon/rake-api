using PasApiClient;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib.Parsing
{
    internal class Parser
    {
        private static readonly string _grammar = GetResource("Grammar.pas");
        private static readonly ParsedExpression[] _emptyComputeArray = new ParsedExpression[0];

        private readonly ParserClient _parserClient =
            ParserClient.CreateFromBaseUri(new Uri("http://pas-api.dev.vplauzon.com/"));

        public Parser(IEnumerable<string> predefinedVariables = null)
        {
            PredefinedVariables = predefinedVariables == null
                ? DefaultEnvironment.PredefinedVariableNames
                : ImmutableList<string>.Empty.AddRange(predefinedVariables).ToImmutableSortedSet();
        }

        public IImmutableSet<string> PredefinedVariables { get; }

        public async Task<ParsedExpression> ParseExpressionAsync(string expression)
        {
            var result = await _parserClient.SingleParseAsync(_grammar, "expression", expression);

            if (result.IsMatch)
            {
                return ParseExpression(result.RuleMatch);
            }
            else
            {
                return null;
            }
        }

        public async Task<ParsedFunction> ParseFunctionAsync(FunctionDescription description)
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
                                ParseExpression(v.Value.RuleMatch));
            var outputs = from o in namedOutputResults
                          select KeyValuePair.Create(
                              o.Key,
                              ParseExpression(o.Value.RuleMatch));

            return new ParsedFunction
            {
                Inputs = description.Inputs,
                Variables = new Dictionary<string, ParsedExpression>(variables),
                Outputs = new Dictionary<string, ParsedExpression>(outputs)
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

            //  Checking inputs
            if (description.Inputs.Any(i => string.IsNullOrWhiteSpace(i)))
            {
                throw new ComputeException("Inputs can't be blank");
            }
            if (description.Inputs.Any(i => !nameValidator(i)))
            {
                throw new ComputeException("Field names can only have alphanumeric characters and underscores");
            }
            if (description.Variables.Keys.Any(i => PredefinedVariables.Contains(i)))
            {
                throw new ComputeException(
                    $"Field names cannot use any of the reserved keywords:  {{{string.Join(", ", PredefinedVariables)}}}");
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
            if (description.Variables.Keys.Any(i => PredefinedVariables.Contains(i)))
            {
                throw new ComputeException(
                    $"Field names cannot use any of the reserved keywords:  {{{string.Join(", ", PredefinedVariables)}}}");
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

            //  Check unicity of Input, Variables & Outputs
            var repeatedInputInVariable =
                description.Inputs.Intersect(description.Variables.Keys).FirstOrDefault();
            var repeatedInputInOutput =
                description.Inputs.Intersect(description.Outputs.Keys).FirstOrDefault();
            var repeatedVariableInOutput =
                description.Variables.Keys.Intersect(description.Outputs.Keys).FirstOrDefault();

            if (repeatedInputInVariable != null)
            {
                throw new ComputeException($"Variable '{repeatedInputInVariable}' has the same name as an input");
            }
            if (repeatedInputInOutput != null)
            {
                throw new ComputeException($"Output '{repeatedInputInOutput}' has the same name as an input");
            }
            if (repeatedVariableInOutput != null)
            {
                throw new ComputeException($"Output '{repeatedVariableInOutput}' has the same name as a variable");
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
            var type = typeof(RakeLib.Parsing.Parser);
            var assembly = type.GetTypeInfo().Assembly;
            var fullResourceName = $"{type.Namespace}.{resourceName}";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }

        private ParsedExpression ParseExpression(RuleMatchResult ruleMatch)
        {
            var objectResult = ruleMatch.NamedChildren["obj"];
            var objectExpression = GetObjectExpression(objectResult);
            var invokes = ruleMatch.NamedChildren["meth"].Children;
            var expression = ParseMethodPropertyInvokes(objectExpression, invokes);

            return expression;
        }

        private ParsedExpression GetObjectExpression(RuleMatchResult ruleMatch)
        {
            var child = ruleMatch.NamedChildren.First();
            var objectType = child.Key;
            var objectValue = child.Value;

            switch (objectType)
            {
                case "prim":
                    return new ParsedExpression { Primitive = ParsePrimitive(objectValue) };
                case "ref":
                    return new ParsedExpression { Reference = objectValue.Text };
                default:
                    throw new NotSupportedException($"Object '{objectType}'");
            }
        }

        private ParsedPrimitive ParsePrimitive(RuleMatchResult ruleMatch)
        {
            var child = ruleMatch.NamedChildren.First();
            var primitiveType = child.Key;
            var primitive = child.Value;

            switch (primitiveType)
            {
                case "int":
                    return new ParsedPrimitive { Integer = int.Parse(primitive.Text) };
                case "string":
                    return new ParsedPrimitive { QuotedString = primitive.NamedChildren["s"].Text };
                default:
                    throw new NotSupportedException($"Primitive '{primitiveType}'");
            }
        }

        private ParsedExpression ParseMethodPropertyInvokes(
            ParsedExpression objectExpression,
            IEnumerable<RuleMatchResult> invokes)
        {
            if (invokes == null || !invokes.Any())
            {
                return objectExpression;
            }
            else
            {
                var invoke = invokes.First();
                var invokeExpression = ParseOneMethodPropertyInvoke(objectExpression, invoke);

                //  Recurse
                return ParseMethodPropertyInvokes(invokeExpression, invokes.Skip(1));
            }
        }

        private ParsedExpression ParseOneMethodPropertyInvoke(
            ParsedExpression objectExpression,
            RuleMatchResult ruleMatch)
        {
            var name = ruleMatch.NamedChildren["name"].Text;
            var parameterListOption = ruleMatch.NamedChildren["params"];

            if (parameterListOption.Children == null)
            {
                return new ParsedExpression
                {
                    Property = new ParsedProperty { Object = objectExpression, Name = name }
                };
            }
            else
            {
                var parameterList = parameterListOption.Children.First();
                var paramType = parameterList.NamedChildren.First().Key;

                switch (paramType)
                {
                    case "empty":
                        return new ParsedExpression
                        {
                            MethodInvoke = new ParsedMethodInvoke
                            {
                                Object = objectExpression,
                                Name = name,
                                Parameters = _emptyComputeArray
                            }
                        };
                    case "nonEmpty":
                        return new ParsedExpression
                        {
                            MethodInvoke = new ParsedMethodInvoke
                            {
                                Object = objectExpression,
                                Name = name,
                                Parameters = ParseParameters(parameterList.NamedChildren.First().Value)
                            }
                        };
                    default:
                        throw new NotSupportedException($"Parameter type '{paramType}'");
                }
            }
        }

        private ParsedExpression[] ParseParameters(RuleMatchResult ruleMatch)
        {
            var head = ruleMatch.NamedChildren["head"];
            var tail = ruleMatch.NamedChildren["tail"];
            var headExpression = ParseExpression(head);

            if (tail.Children != null)
            {
                var tailExpressions = from match in tail.Children
                                      select ParseExpression(match.NamedChildren["e"]);
                var parameters = tailExpressions.Prepend(headExpression).ToArray();

                return parameters;
            }
            else
            {
                return new[] { headExpression };
            }
        }
    }
}