using System;
using System.Collections.Generic;

namespace RakeLib
{
    public class Function<T>
    {
        public string[] Inputs { get; set; }

        public IDictionary<string, T> Variables { get; set; }

        public IDictionary<string, T> Outputs { get; set; }
    }
}