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
            private readonly IImmutableDictionary<string, string> _inputs;
            private readonly CompiledFunction _function;

            public EngineStateMachine(
                Quotas quotas,
                IImmutableDictionary<string, string> inputs,
                CompiledFunction function)
            {
                _quotas = quotas;
                _inputs = inputs;
                _function = function;
            }

            public async Task<ComputeResult> ComputeAsync()
            {
                await Task.CompletedTask;

                throw new NotImplementedException();
            }
        }
        #endregion

        private readonly Quotas _quotas;

        public ComputeEngine(Quotas quotas)
        {
            _quotas = quotas ?? throw new ArgumentNullException(nameof(quotas));
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

            var stateMachine = new EngineStateMachine(_quotas, inputs, function);
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