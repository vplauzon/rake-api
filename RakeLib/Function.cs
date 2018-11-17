using System;
using System.Collections.Generic;

namespace RakeLib
{
    public class Function<T>
    {
        public string ApiVersion { get; set; }

        public string[] Inputs { get; set; }

        public Variable<T>[] Variables { get; set; }

        public IDictionary<string, T> Outputs { get; set; }
    }
}