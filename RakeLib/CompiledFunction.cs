using System;
using System.Collections.Generic;

namespace RakeLib
{
    public class CompiledFunction
    {
        public string[] Inputs { get; set; }

        public NamedCompiledCompute[] Computes { get; set; }
    }
}