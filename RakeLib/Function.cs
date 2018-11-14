﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Function
    {
        public string[] Inputs { get; set; }

        public VariableDescription[] Variables { get; set; }

        public IDictionary<string, Func<int, Task<string>>> Outputs { get; set; }
    }
}