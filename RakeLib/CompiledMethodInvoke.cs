using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RakeLib
{
    public class CompiledMethodInvoke
    {
        public string ObjectReference { get; set; }

        public string Name { get; set; }

        public string[] Parameters { get; set; }

        #region Object Methods
        public override string ToString()
        {
            return $"{ObjectReference}.{Name}({string.Join(", ", Parameters.Select(p => p.ToString()))})";
        }

        public override bool Equals(object obj)
        {
            var method = obj as CompiledMethodInvoke;

            return method != null
                && object.Equals(ObjectReference, method.ObjectReference)
                && object.Equals(Name, method.Name)
                && Parameters.Zip(method.Parameters, (p1, p2) => object.Equals(p1, p2)).All(p => p == true);
        }

        public override int GetHashCode()
        {
            return ObjectReference.GetHashCode()
                ^ Name.GetHashCode()
                ^ Parameters.Aggregate(0, (h, s) => h ^ s.GetHashCode());
        }
        #endregion
    }
}