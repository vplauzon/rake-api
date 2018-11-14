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
            if (quotas == null)
            {
                throw new ArgumentNullException(nameof(quotas));
            }
        }

        public async Task<IImmutableDictionary<string, string>> ComputeAsync(
            IDictionary<string, string> inputs,
            Function function)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }
            ValidateInputs(inputs, function.Inputs);

            var context = new ComputeContext(inputs);
            var outputs = ImmutableDictionary<string, string>.Empty;

            foreach (var variable in function.Variables)
            {
                var value = await variable.Compute.ComputeAsync(context);

                context = context.AddVariable(variable.Name, value);
            }
            foreach(var output in function.Outputs)
            {
                var value = await output.Value.ComputeAsync(context);

                outputs.Add(output.Key, value);
            }

            return outputs;
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