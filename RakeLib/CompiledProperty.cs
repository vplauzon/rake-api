using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class CompiledProperty
    {
        public string ObjectReference { get; set; }

        public string Name { get; set; }

        #region Object Methods
        public override string ToString()
        {
            return $"{ObjectReference}.{Name}";
        }

        public override bool Equals(object obj)
        {
            var property = obj as CompiledProperty;

            return property != null
                && object.Equals(ObjectReference, property.ObjectReference)
                && object.Equals(Name, property.Name);
        }

        public override int GetHashCode()
        {
            return ObjectReference.GetHashCode() ^ Name.GetHashCode();
        }
        #endregion
    }
}