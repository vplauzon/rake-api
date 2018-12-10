using System;
using System.Collections.Generic;

namespace RakeLib
{
    public class CompiledFunction
    {
        public string[] InputNames { get; set; }

        public NamedCompiledCompute[] Computes { get; set; }
    }
}