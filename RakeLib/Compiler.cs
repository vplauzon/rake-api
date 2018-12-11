using RakeLib.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Compiler
    {
        #region Inner Types
        private class CompilerStateMachine
        {
            private static readonly IImmutableSet<string> _predefinedCompute =
                ImmutableSortedSet<string>
                .Empty
                .Add("url")
                .Add("content")
                .Add("util");

            private readonly IImmutableSet<string> _inputSet;
            private readonly IImmutableDictionary<string, ParsedExpression> _variables;
            private readonly IImmutableDictionary<string, ParsedExpression> _outputs;

            private IImmutableSet<string> _variablesInProcessSet = ImmutableSortedSet<string>.Empty;
            private IImmutableList<NamedCompiledCompute> _compiledComputes =
                ImmutableList<NamedCompiledCompute>.Empty;
            //private IImmutableSet<string> _compiledComputesIndex = ImmutableSortedSet<string>.Empty;
            //private int _hiddenVariableIndex = 1;

            public CompilerStateMachine(ParsedFunction parsedFunction)
            {
                _inputSet = ImmutableList<string>
                    .Empty
                    .AddRange(parsedFunction.Inputs)
                    .ToImmutableSortedSet();
                _variables = ImmutableSortedDictionary<string, ParsedExpression>
                    .Empty
                    .AddRange(parsedFunction.Variables);
                _outputs = ImmutableSortedDictionary<string, ParsedExpression>
                    .Empty
                    .AddRange(parsedFunction.Outputs);
            }

            public CompiledFunction Compile()
            {
                foreach (var output in _outputs)
                {
                    var compiledCompute = CompileExpression(output.Value);

                    PushOutputCompute(output.Key, compiledCompute);
                }

                return new CompiledFunction
                {
                    InputNames = _inputSet.ToArray(),
                    Computes = _compiledComputes.ToArray()
                };
            }

            private CompiledCompute CompileExpression(ParsedExpression parsedCompute)
            {
                if (parsedCompute.Primitive != null)
                {
                    return CompilePrimitive(parsedCompute.Primitive);
                }
                else if (parsedCompute.Reference != null)
                {
                    return CompileReference(parsedCompute.Reference);
                }
                else if (parsedCompute.Property != null)
                {
                    return CompileProperty(parsedCompute.Property);
                }
                else if (parsedCompute.MethodInvoke != null)
                {
                    return CompileMethodInvoke(parsedCompute.MethodInvoke);
                }
                else
                {
                    throw new NotSupportedException("Parsed Expression is empty");
                }
            }

            private CompiledCompute CompilePrimitive(ParsedPrimitive primitive)
            {
                return new CompiledCompute
                {
                    Primitive = new CompiledPrimitive
                    {
                        Integer = primitive.Integer,
                        QuotedString = primitive.QuotedString
                    }
                };
            }

            private CompiledCompute CompileReference(string reference)
            {
                if (_inputSet.Contains(reference))
                {
                    return new CompiledCompute
                    {
                        InputReference = reference
                    };
                }
                else if (_variables.ContainsKey(reference))
                {
                    EnsureVariable(reference);

                    return new CompiledCompute
                    {
                        NamedComputeReference = reference
                    };
                }
                else
                {
                    throw new ComputeException($"Referenced field '{reference}' isn't a declared input or variable");
                }
            }

            private CompiledCompute CompileProperty(ParsedProperty property)
            {
                var compiledObject = CompileExpression(property.Object);
                var objectReference = PushIntermediaryCompute(compiledObject);

                return new CompiledCompute
                {
                    Property = new CompiledProperty
                    {
                        Name = property.Name,
                        ObjectReference = objectReference
                    }
                };
            }

            private CompiledCompute CompileMethodInvoke(ParsedMethodInvoke methodInvoke)
            {
                var compiledObject = CompileExpression(methodInvoke.Object);
                var objectReference = PushIntermediaryCompute(compiledObject);
                var parameterReferences = (from p in methodInvoke.Parameters
                                           let compiled = CompileExpression(p)
                                           select PushIntermediaryCompute(compiled)).ToArray();

                return new CompiledCompute
                {
                    MethodInvoke = new CompiledMethodInvoke
                    {
                        Name = methodInvoke.Name,
                        ObjectReference = objectReference,
                        Parameters = parameterReferences
                    }
                };
            }

            private void EnsureVariable(string name)
            {
                if (!IsVariableComputed(name))
                {
                    if (_variablesInProcessSet.Contains(name))
                    {
                        throw new ComputeException($"Circular reference with variable '{name}'");
                    }
                    else
                    {
                        StartVariableProcess(name);

                        var compiledVariable = CompileExpression(_variables[name]);

                        PushVariableCompute(name, compiledVariable);
                        StopVariableProcess(name);
                    }
                }
            }

            private void PushVariableCompute(string name, CompiledCompute compiledVariable)
            {
                throw new NotImplementedException();
            }

            private string PushIntermediaryCompute(CompiledCompute compiledObject)
            {
                throw new NotImplementedException();
            }

            private void PushOutputCompute(string key, CompiledCompute compiledCompute)
            {
                throw new NotImplementedException();
            }

            private void StartVariableProcess(string name)
            {
                _variablesInProcessSet = _variablesInProcessSet.Add(name);
            }

            private void StopVariableProcess(string name)
            {
                _variablesInProcessSet = _variablesInProcessSet.Remove(name);
            }

            private bool IsVariableComputed(string name)
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        private readonly Parser _parser = new Parser();

        public async Task<CompiledFunction> CompileAsync(FunctionDescription description)
        {
            var parsed = await _parser.ParseFunctionAsync(description);
            var stateMachine = new CompilerStateMachine(parsed);
            var compiledFunction = stateMachine.Compile();

            return compiledFunction;
        }
    }
}