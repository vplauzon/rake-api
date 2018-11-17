using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace RakeLib
{
    public class ExecutableFunction
    {
        public ExecutableFunction(
            IEnumerable<string> inputs,
            IEnumerable<ExecutableVariable> variables,
            IDictionary<string, IOutputCompute> outputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }
            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }
            if (outputs == null || outputs.Count == 0)
            {
                throw new ArgumentNullException(nameof(outputs));
            }
            Inputs = ImmutableArray<string>.Empty.AddRange(inputs);
            Variables = ImmutableArray<ExecutableVariable>.Empty.AddRange(variables);
            Outputs = ImmutableDictionary<string, IOutputCompute>.Empty.AddRange(outputs);
        }

        public IImmutableList<string> Inputs { get; }

        public IImmutableList<ExecutableVariable> Variables { get; }

        public IImmutableDictionary<string, IOutputCompute> Outputs { get; }
    }
}