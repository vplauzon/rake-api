using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class MethodSet
    {
        #region Construction methods
        public static MethodSet Empty { get; } = new MethodSet();

        private MethodSet()
        {
        }

        public MethodSet AddMethodsAndPropertiesByReflection<T>()
        {
            throw new NotImplementedException();
        }

        public MethodSet AddMethodResolver(IMethodResolver resolver)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Consuming Methods
        public Task<object> ComputePropertyAsync(object target, string name)
        {
            throw new NotImplementedException();
        }

        public Task<object> ComputeMethodAsync(object target, string name, IEnumerable<object> parameters)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}