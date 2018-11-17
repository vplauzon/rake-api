using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class Variable<T>
    {
        public string Name { get; set; }

        public T Description { get; set; }
    }
}