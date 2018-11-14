﻿using System;
using System.Collections.Generic;

namespace rake_lib
{
    public class FunctionDescription
    {
        public string[] Inputs { get; set; }

        public VariableDescription[] Variables { get; set; }

        public IDictionary<string, string> Outputs { get; set; }
    }
}