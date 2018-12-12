using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    public class ComputeResult
    {
        public ComputeResult(
            IImmutableDictionary<string, object> variables,
            IImmutableDictionary<string, object> outputs)
        {
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
            Outputs = outputs ?? throw new ArgumentNullException(nameof(outputs));
        }

        public IImmutableDictionary<string, object> Variables { get; }

        public IImmutableDictionary<string, object> Outputs { get; }
    }
}