using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        public CompiledPrimitive Primitive { get; set; }

        public string InputReference { get; set; }

        public string NamedComputeReference { get; set; }

        public CompiledProperty Property { get; set; }

        public CompiledMethodInvoke MethodInvoke { get; set; }
    }
}