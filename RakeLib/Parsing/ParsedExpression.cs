using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib.Parsing
{
    internal class ParsedExpression
    {
        public ParsedPrimitive Primitive { get; set; }

        public string Reference { get; set; }
        
        public ParsedProperty Property { get; set; }

        public ParsedMethodInvoke MethodInvoke { get; set; }
    }
}