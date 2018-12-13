using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class FunctionDescription
    {
        public string[] Inputs { get; set; }

        public IDictionary<string, string> Variables { get; set; }

        public IDictionary<string, string> Outputs { get; set; }
    }
}