using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class NamedCompiledCompute
    {
        public string Name { get; set; }

        public bool IsOutput { get; set; }

        public bool IsHidden { get; set; }

        public CompiledCompute Compute { get; set; }
    }
}