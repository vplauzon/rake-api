using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class NamedCompiledCompute
    {
        public string Name { get; set; }

        public bool IsOutput { get; set; }

        public bool IsDeclaredVariable { get; set; }

        public bool IsExecutionTimeInjected { get; set; }

        public CompiledCompute Compute { get; set; }

        #region Object Methods
        public override string ToString()
        {
            return $"{Name} = {Compute}";
        }
        #endregion
    }
}