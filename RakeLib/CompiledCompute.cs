using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        public CompiledReference Reference { get; set; }

        public CompiledMethodInvoke MethodInvoke { get; set; }
    }
}