using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        //  Primitive
        //  Input Ref
        //  Compute Ref
        //  Property
        //  Method Invoke
        public bool IsProperty { get; set; }

        public string Name { get; set; }

        public CompiledReference[] Parameters { get; set; }
    }
}