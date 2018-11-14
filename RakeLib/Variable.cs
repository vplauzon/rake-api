using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Variable
    {
        public string Name { get; set; }

        public IOutputCompute Compute { get; set; }
    }
}