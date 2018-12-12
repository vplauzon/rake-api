using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledCompute
    {
        public CompiledPrimitive Primitive { get; set; }

        public string InputReference { get; set; }

        public string NamedComputeReference { get; set; }

        public CompiledProperty Property { get; set; }

        public CompiledMethodInvoke MethodInvoke { get; set; }

        #region Object Methods
        public override string ToString()
        {
            if (Primitive != null)
            {
                return Primitive.ToString();
            }
            else if (InputReference != null)
            {
                return $"[Input]({InputReference})";
            }
            else if (NamedComputeReference != null)
            {
                return $"[Compute]({NamedComputeReference})";
            }
            else if (Property != null)
            {
                return Property.ToString();
            }
            else if (MethodInvoke != null)
            {
                return MethodInvoke.ToString();
            }
            else
            {
                return "**Undefined Compute**";
            }
        }

        public override bool Equals(object obj)
        {
            var compute = obj as CompiledCompute;

            return compute != null
                && object.Equals(Primitive, compute.Primitive)
                && object.Equals(InputReference, compute.InputReference)
                && object.Equals(NamedComputeReference, compute.NamedComputeReference)
                && object.Equals(Property, compute.Property)
                && object.Equals(MethodInvoke, compute.MethodInvoke);
        }

        public override int GetHashCode()
        {
            if (Primitive != null)
            {
                return Primitive.GetHashCode();
            }
            else if (InputReference != null)
            {
                return InputReference.GetHashCode();
            }
            else if (NamedComputeReference != null)
            {
                return NamedComputeReference.GetHashCode();
            }
            else if (Property != null)
            {
                return Property.GetHashCode();
            }
            else if (MethodInvoke != null)
            {
                return MethodInvoke.GetHashCode();
            }
            else
            {
                return 0;
            }
        }
        #endregion
    }
}