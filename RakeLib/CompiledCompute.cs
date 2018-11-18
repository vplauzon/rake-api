using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        public int? Integer { get; set; }

        public string QuotedString { get; set; }

        public string Identifier { get; set; }

        public CompiledMethodInvoke MethodInvoke { get; set; }
    }
}