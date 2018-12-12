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
            private readonly IImmutableSet<string> _inputSet;
            private readonly IImmutableDictionary<string, ParsedExpression> _variables;
            private readonly IImmutableDictionary<string, ParsedExpression> _outputs;
            private readonly IImmutableSet<string> _predefinedVariables;

            private IImmutableSet<string> _variablesInProcessSet = ImmutableSortedSet<string>.Empty;
            private IImmutableSet<string> _variablesProcessedSet = ImmutableSortedSet<string>.Empty;
            private IImmutableList<NamedCompiledCompute> _compiledComputes =
                ImmutableList<NamedCompiledCompute>.Empty;
            private IImmutableDictionary<CompiledCompute, string> _computeToNameMap =
                ImmutableDictionary<CompiledCompute, string>.Empty;
            private int _intermediaryVariableIndex = 1;

            public CompilerStateMachine(ParsedFunction parsedFunction, IImmutableSet<string> predefinedVariables)
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
                _predefinedVariables = predefinedVariables;
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
                else if (_predefinedVariables.Contains(reference))
                {
                    EnsurePredefinedVariable(reference);

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

            private void EnsurePredefinedVariable(string name)
            {
                if (!IsVariableComputed(name))
                {
                    PushPredefinedCompute(name);
                }
            }

            private bool IsVariableComputed(string name)
            {
                return _variablesProcessedSet.Contains(name);
            }

            private void PushVariableCompute(string name, CompiledCompute compiledVariable)
            {
                var existingComputeReference = FindCompiledComputeName(compiledVariable);

                if (existingComputeReference != null)
                {
                    compiledVariable = new CompiledCompute
                    {
                        NamedComputeReference = existingComputeReference
                    };
                }

                _variablesProcessedSet = _variablesProcessedSet.Add(name);
                PushNamedCompute(new NamedCompiledCompute
                {
                    Name = name,
                    Compute = compiledVariable,
                    IsDeclaredVariable = true,
                    IsExecutionTimeInjected = false,
                    IsOutput = false
                });
            }

            private string PushIntermediaryCompute(CompiledCompute compiledObject)
            {
                var existingComputeReference = FindCompiledComputeName(compiledObject);

                if (existingComputeReference != null)
                {
                    return existingComputeReference;
                }
                else
                {
                    var name = "$" + (_intermediaryVariableIndex++).ToString();

                    PushNamedCompute(new NamedCompiledCompute
                    {
                        Name = name,
                        Compute = compiledObject,
                        IsDeclaredVariable = false,
                        IsExecutionTimeInjected = false,
                        IsOutput = false
                    });

                    return name;
                }
            }

            private void PushOutputCompute(string name, CompiledCompute outputCompute)
            {
                var existingComputeReference = FindCompiledComputeName(outputCompute);

                if (existingComputeReference != null)
                {
                    outputCompute = new CompiledCompute
                    {
                        NamedComputeReference = existingComputeReference
                    };
                }

                PushNamedCompute(new NamedCompiledCompute
                {
                    Name = name,
                    Compute = outputCompute,
                    IsDeclaredVariable = false,
                    IsExecutionTimeInjected = false,
                    IsOutput = true
                });
            }

            private void PushPredefinedCompute(string name)
            {
                PushNamedCompute(new NamedCompiledCompute
                {
                    Name = name,
                    Compute = null,
                    IsDeclaredVariable = false,
                    IsExecutionTimeInjected = true,
                    IsOutput = false
                });
            }

            private void PushNamedCompute(NamedCompiledCompute namedCompiledCompute)
            {
                _compiledComputes = _compiledComputes.Add(namedCompiledCompute);
                if (namedCompiledCompute.Compute != null)
                {
                    _computeToNameMap = _computeToNameMap.Add(namedCompiledCompute.Compute, namedCompiledCompute.Name);
                }
            }

            private string FindCompiledComputeName(CompiledCompute compiledCompute)
            {
                _computeToNameMap.TryGetValue(compiledCompute, out string name);

                return name;
            }

            private void StartVariableProcess(string name)
            {
                _variablesInProcessSet = _variablesInProcessSet.Add(name);
            }

            private void StopVariableProcess(string name)
            {
                _variablesInProcessSet = _variablesInProcessSet.Remove(name);
            }
        }
        #endregion

        private readonly Parser _parser;

        public Compiler(IEnumerable<string> predefinedVariables = null)
        {
            _parser = new Parser(predefinedVariables);
        }

        public async Task<CompiledFunction> CompileAsync(FunctionDescription description)
        {
            var parsed = await _parser.ParseFunctionAsync(description);
            var stateMachine = new CompilerStateMachine(parsed, _parser.PredefinedVariables);
            var compiledFunction = stateMachine.Compile();

            return compiledFunction;
        }
    }
}