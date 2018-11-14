using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Function
    {
        public ImmutableArray<string> Inputs { get; set; }

        public ImmutableArray<Variable> Variables { get; set; }

        public IImmutableDictionary<string, IOutputCompute> Outputs { get; set; }
    }
}