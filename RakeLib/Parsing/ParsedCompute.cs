using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib.Parsing
{
    internal class ParsedCompute
    {
        public ParsedReference Reference { get; set; }

        public ParsedMethodInvoke MethodInvoke { get; set; }
    }
}