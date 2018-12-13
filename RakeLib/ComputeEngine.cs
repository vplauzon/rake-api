using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class ComputeEngine
    {
        #region Inner Types
        private class EngineStateMachine
        {
            private readonly Quotas _quotas;
            private readonly MethodSet _methodSet;
            private readonly IImmutableDictionary<string, object> _predefinedVariables;
            private readonly IImmutableDictionary<string, string> _inputs;
            private readonly CompiledFunction _function;

            private IImmutableDictionary<string, object> _computeResult = ImmutableDictionary<string, object>.Empty;
            private IImmutableDictionary<string, object> _variables = ImmutableDictionary<string, object>.Empty;
            private IImmutableDictionary<string, object> _outputs = ImmutableDictionary<string, object>.Empty;

            public EngineStateMachine(
                Quotas quotas,
                MethodSet methodSet,
                IImmutableDictionary<string, object> predefinedVariables,
                IImmutableDictionary<string, string> inputs,
                CompiledFunction function)
            {
                _quotas = quotas;
                _methodSet = methodSet;
                _predefinedVariables = predefinedVariables;
                _inputs = inputs;
                _function = function;
            }

            public async Task<ComputeResult> ComputeAsync()
            {
                foreach (var function in _function.Computes)
                {
                    var result = await ComputeFunctionAsync(function);

                    _computeResult = _computeResult.Add(function.Name, result);
                    if (function.IsDeclaredVariable)
                    {
                        _variables = _variables.Add(function.Name, result);
                    }
                    if (function.IsOutput)
                    {
                        _outputs = _outputs.Add(function.Name, result);
                    }
                }

                return new ComputeResult(_variables, _outputs);
            }

            private async Task<object> ComputeFunctionAsync(NamedCompiledCompute function)
            {
                await Task.CompletedTask;

                if (function.IsExecutionTimeInjected)
                {
                    return ComputePredefinedVariable(function.Name);
                }
                else
                {
                    var compute = function.Compute;

                    if (compute.Primitive != null)
                    {
                        return ComputePrimitive(compute.Primitive);
                    }
                    else if (compute.InputReference != null)
                    {
                        return ComputeInputReference(compute.InputReference);
                    }
                    else if (compute.NamedComputeReference != null)
                    {
                        return ComputeNamedComputeReference(compute.NamedComputeReference);
                    }
                    else if (compute.Property != null)
                    {
                        return await ComputePropertyAsync(compute.Property);
                    }
                    else if (compute.MethodInvoke != null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException("Invalid compute");
                    }
                }
            }

            private object ComputePrimitive(CompiledPrimitive primitive)
            {
                if (primitive.Integer != null)
                {
                    return primitive.Integer.Value;
                }
                else if (primitive.QuotedString != null)
                {
                    return primitive.QuotedString;
                }
                else
                {
                    throw new NotImplementedException("Invalid primitive");
                }
            }

            private string ComputeInputReference(string inputReference)
            {
                return _inputs[inputReference];
            }

            private object ComputeNamedComputeReference(string namedComputeReference)
            {
                return _computeResult[namedComputeReference];
            }

            private async Task<object> ComputePropertyAsync(CompiledProperty property)
            {
                var objectReference = _computeResult[property.ObjectReference];
                var result = await _methodSet.ComputePropertyAsync(objectReference, property.Name);

                return result;
            }

            private object ComputePredefinedVariable(string name)
            {
                if (_predefinedVariables.TryGetValue(name, out var value))
                {
                    return value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Predefined variable '{name}' isn't available at runtime");
                }
            }
        }
        #endregion

        private readonly Quotas _quotas;
        private readonly MethodSet _methodSet;
        private readonly IImmutableDictionary<string, object> _predefinedVariables;

        public ComputeEngine(
            Quotas quotas,
            MethodSet methodSet = null,
            IImmutableDictionary<string, object> predefinedVariables = null)
        {
            _quotas = quotas ?? throw new ArgumentNullException(nameof(quotas));
            _methodSet = methodSet ?? MethodSet.Empty;
            _predefinedVariables = predefinedVariables ?? DefaultEnvironment.PredefinedVariables;
        }

        public async Task<ComputeResult> ComputeAsync(
            IImmutableDictionary<string, string> inputs,
            CompiledFunction function)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }
            ValidateInputs(inputs.Keys, function.InputNames);

            var stateMachine = new EngineStateMachine(_quotas, _methodSet, _predefinedVariables, inputs, function);
            var result = await stateMachine.ComputeAsync();

            return result;
        }

        private void ValidateInputs(
            IEnumerable<string> providedInputs,
            IEnumerable<string> functionInputs)
        {
            var overSpec = Enumerable.Except(providedInputs, functionInputs);

            if (overSpec.Any())
            {
                throw new ComputeException(
                    "The following inputs were provided but aren't part of the function specs:  "
                    + string.Join(", ", overSpec));
            }
            else
            {
                var underSpec = Enumerable.Except(functionInputs, providedInputs);

                if (underSpec.Any())
                {
                    throw new ComputeException(
                        "The following inputs were not provided but are part of the function specs:  "
                        + string.Join(", ", underSpec));
                }
            }
        }
    }
}