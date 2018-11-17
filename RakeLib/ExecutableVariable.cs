using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class ExecutableVariable
    {
        public ExecutableVariable(string name, IOutputCompute compute)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
            Compute = compute ?? throw new ArgumentNullException(nameof(compute));
        }

        public string Name { get; }

        public IOutputCompute Compute { get; }
    }
}