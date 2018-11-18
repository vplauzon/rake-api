using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        public string Identifier { get; set; }

        public CompiledMethodInvoke MethodInvoke { get; set; }
    }
}