using System;
using System.Collections.Generic;
using System.Linq;

namespace RakeLib
{
    public class CompiledFunction
    {
        public string[] InputNames { get; set; }

        public NamedCompiledCompute[] Computes { get; set; }

        #region Object Methods
        public override string ToString()
        {
            return $"Inputs:  {{{string.Join(", ", InputNames)}}}"
                + Environment.NewLine
                + string.Join(Environment.NewLine, Computes.Select(c => c.ToString()));
        }
        #endregion
    }
}