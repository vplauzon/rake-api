using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    public class ComputeResult
    {
        public ComputeResult()
        {
        }

        public IImmutableDictionary<string, object> Variables { get; }

        public IImmutableDictionary<string, object> Outputs { get; }
    }
}