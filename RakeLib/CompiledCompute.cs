using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        public bool IsProperty { get; set; }

        public string Name { get; set; }

        public CompiledReference[] Parameters { get; set; }
    }
}