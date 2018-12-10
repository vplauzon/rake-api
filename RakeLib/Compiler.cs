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

            private readonly string[] _inputs;
            private readonly IImmutableSet<string> _inputSet;
            private readonly IDictionary<string, ParsedCompute> _variables;
            private readonly IDictionary<string, ParsedCompute> _outputs;
            private IImmutableStack<string> _computeStack = ImmutableStack<string>.Empty;
            private IImmutableList<NamedCompiledCompute> _compiledComputes =
                ImmutableList<NamedCompiledCompute>.Empty;
            private IImmutableSet<string> _compiledComputesIndex =
                ImmutableSortedSet<string>.Empty;
            //private int _hiddenVariableIndex = 1;

            public CompilerStateMachine(ParsedFunction parsedFunction)
            {
                _inputs = parsedFunction.Inputs;
                _inputSet = ImmutableList<string>
                    .Empty
                    .AddRange(parsedFunction.Inputs)
                    .ToImmutableSortedSet();
                _variables = ImmutableSortedDictionary<string, ParsedCompute>
                    .Empty
                    .AddRange(parsedFunction.Variables);
                _outputs = ImmutableSortedDictionary<string, ParsedCompute>
                    .Empty
                    .AddRange(parsedFunction.Outputs);
            }

            public CompiledFunction Compile()
            {
                while (_outputs.Any())
                {
                    CompileOutput(_outputs.Keys.First());
                }

                if (_variables.Any())
                {
                    throw new ComputeException(
                        $"Variable '{_variables.Keys.First()}' isn't used in any output");
                }

                return new CompiledFunction
                {
                    Inputs = _inputs,
                    Computes = _compiledComputes.ToArray()
                };
            }

            private void CompileOutput(string name)
            {
                var parsedOutput = _outputs[name];

                _outputs.Remove(name);

                CompileCompute(name, parsedOutput, true, false);
                _computeStack = _computeStack.Push(name);

                throw new NotImplementedException();
            }

            private void CompileVariable(string name)
            {
                var parsedVariable = _variables[name];

                _variables.Remove(name);

                CompileCompute(name, parsedVariable, false, false);
            }

            private void CompileCompute(
                string name,
                ParsedCompute parsedCompute,
                bool isOutput,
                bool isHidden)
            {
                if (_computeStack.Contains(name))
                {
                    throw new ComputeException($"Circular reference involving '{name}'");
                }
                _computeStack = _computeStack.Push(name);

                var reference = EnsureReference(parsedCompute);

                while (parsedCompute.MethodInvoke != null && parsedCompute.MethodInvoke.Next != null)
                {
                    var compiledCompute =
                        CompileImmediateCompute(reference, parsedCompute.MethodInvoke);
                }


                if (parsedCompute.MethodInvoke == null)
                {
                    var compute = new NamedCompiledCompute
                    {
                        Name = name,
                        Compute = new CompiledCompute
                        {
                            Name = name,
                            Parameters = new[] { reference },
                            IsProperty = false
                        },
                        IsHidden = isHidden,
                        IsOutput = isOutput
                    };

                    AddCompiledCompute(compute);
                }
                else
                {
                    throw new NotImplementedException();
                }
                _computeStack = _computeStack.Pop();
            }

            private CompiledCompute CompileImmediateCompute(
                CompiledReference reference,
                ParsedMethodInvoke methodInvoke)
            {
                throw new NotImplementedException();
            }

            private void AddCompiledCompute(NamedCompiledCompute compute)
            {
                _compiledComputes = _compiledComputes.Add(compute);
                _compiledComputesIndex = _compiledComputesIndex.Add(compute.Name);
            }

            private CompiledReference EnsureReference(ParsedCompute parsedCompute)
            {
                var reference = new CompiledReference
                {
                    Identifier = parsedCompute.Reference.Identifier,
                    Integer = parsedCompute.Reference.Integer,
                    QuotedString = parsedCompute.Reference.QuotedString
                };
                var identifier = reference.Identifier;

                if (identifier != null
                    && !_compiledComputesIndex.Contains(identifier)
                    && !_inputSet.Contains(identifier)
                    && !_predefinedCompute.Contains(identifier))
                {
                    if (_variables.ContainsKey(identifier))
                    {
                        CompileVariable(identifier);
                    }
                    else if (_outputs.ContainsKey(identifier))
                    {
                        CompileOutput(identifier);
                    }
                    else
                    {
                        throw new ComputeException($"Unknown identifier '{identifier}'");
                    }
                }

                return reference;
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