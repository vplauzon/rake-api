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
        public ComputeEngine(Quotas quotas)
        {
        }

        public async Task<IDictionary<string, string>> ComputeAsync(
            IDictionary<string, string> inputs,
            Function function)
        {
            ValidateInputs(inputs, function.Inputs);

            await Task.CompletedTask;

            throw new NotImplementedException();
        }

        private void ValidateInputs(
            IDictionary<string, string> inputs,
            ImmutableArray<string> functionInputs)
        {
            var overSpec = Enumerable.Except(inputs.Keys, functionInputs);

            if (overSpec.Any())
            {
                throw new ComputeException(
                    "The following inputs were provided but aren't part of the function specs:  "
                    + string.Join(", ", overSpec));
            }
            else
            {
                var underSpec = Enumerable.Except(functionInputs, inputs.Keys);

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