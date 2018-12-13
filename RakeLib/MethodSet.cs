using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class MethodSet
    {
        private readonly IImmutableDictionary<Type, IImmutableList<MethodInfo>> _properties;
        private readonly IImmutableDictionary<Type, IImmutableList<MethodInfo>> _methods;

        #region Construction methods
        public static MethodSet Empty { get; } = new MethodSet();

        private MethodSet()
        {
            _properties = ImmutableDictionary<Type, IImmutableList<MethodInfo>>.Empty;
            _methods = ImmutableDictionary<Type, IImmutableList<MethodInfo>>.Empty;
        }

        public MethodSet AddMethodsAndPropertiesByReflection(Type type)
        {
            var allMethods = type.GetMethods(BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
            var properties = from m in allMethods
                             where m.GetParameters().Length == 1
                             && m.Name.EndsWith("Property")
                             group m by m.GetParameters()[0].ParameterType into g
                             select g;
            var methods = from m in allMethods
                          where m.GetParameters().Length > 0
                          && !m.Name.EndsWith("Property")
                          group m by m.GetParameters()[0].ParameterType into g
                          select g;

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