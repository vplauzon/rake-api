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
            private readonly string[] _inputs;
            private readonly IImmutableSet<string> _inputSet;
            private readonly IDictionary<string, ParsedCompute> _variables;
            private readonly IDictionary<string, ParsedCompute> _outputs;
            private IImmutableStack<string> _computeStack = ImmutableStack<string>.Empty;
            private IImmutableList<NamedCompiledCompute> _compiledComputes =
                ImmutableList<NamedCompiledCompute>.Empty;

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
                if(_computeStack.Contains(name))
                {
                    throw new ComputeException($"Circular reference involving '{name}'");
                }
                _computeStack = _computeStack.Push(name);

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