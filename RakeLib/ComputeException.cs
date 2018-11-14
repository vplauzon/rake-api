using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class ComputeException : Exception
    {
        public ComputeException(string message) : base(message)
        {
        }
    }
}
