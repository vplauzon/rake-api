using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib.Parsing
{
    internal class ParsedFunction
    {
        public string[] Inputs { get; set; }

        public IDictionary<string, ParsedExpression> Variables { get; set; }

        public IDictionary<string, ParsedExpression> Outputs { get; set; }
    }
}