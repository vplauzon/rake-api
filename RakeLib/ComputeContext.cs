using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    public class ComputeContext
    {
        public IImmutableDictionary<string, string> Inputs { get; private set; }
    }
}