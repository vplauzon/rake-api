using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RakeLib
{
    public class MethodSet
    {
        #region Inner Types
        private class MethodIndex
        {
            private readonly IImmutableDictionary<string, MethodInfo> _index;

            public static MethodIndex Empty { get; } =
                new MethodIndex(ImmutableSortedDictionary<string, MethodInfo>.Empty);

            private MethodIndex(IImmutableDictionary<string, MethodInfo> index)
            {
                _index = index;
            }

            public MethodInfo GetOperation(string name)
            {
                if (_index.TryGetValue(name, out var method))
                {
                    return method;
                }
                else
                {
                    return null;
                }
            }

            public MethodIndex AddRange(IEnumerable<KeyValuePair<string, MethodInfo>> methods)
            {
                var newIndex = _index.AddRange(methods);

                return new MethodIndex(newIndex);
            }

            public MethodIndex Merge(MethodIndex index)
            {
                return AddRange(index._index);
            }
        }

        private class TypeIndex
        {
            private readonly IImmutableDictionary<Type, MethodIndex> _index;

            public static TypeIndex Empty { get; } = new TypeIndex(ImmutableDictionary<Type, MethodIndex>.Empty);

            private TypeIndex(IImmutableDictionary<Type, MethodIndex> index)
            {
                _index = index;
            }

            public MethodInfo GetOperation(Type type, string name)
            {
                if (_index.TryGetValue(type, out var methodIndex))
                {
                    return methodIndex.GetOperation(name);
                }

                return null;
            }

            public TypeIndex MergeWith(IEnumerable<KeyValuePair<Type, MethodIndex>> range)
            {
                var groupedByType = from p in _index.Concat(range)
                                    group p by p.Key into g
                                    select new { Type = g.Key, Methods = g.Select(p => p.Value) };
                var mergedPairs = from g in groupedByType
                                  let mergedMethods = g.Methods.Aggregate((l1, l2) => l1.Merge(l2))
                                  select KeyValuePair.Create(g.Type, mergedMethods);
                var mergedIndex = mergedPairs.ToImmutableDictionary();

                return new TypeIndex(mergedIndex);
            }
        }
        #endregion

        private readonly TypeIndex _properties;
        private readonly TypeIndex _methods;

        #region Construction methods
        public static MethodSet Empty { get; } = new MethodSet(TypeIndex.Empty, TypeIndex.Empty);

        private MethodSet(TypeIndex properties, TypeIndex methods)
        {
            _properties = properties;
            _methods = methods;
        }

        public MethodSet AddMethodsAndPropertiesByReflection(Type type)
        {
            var allMethods = type.GetMethods(BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
            var properties = from m in allMethods
                             where m.GetParameters().Length == 1
                             && m.Name.EndsWith("Property")
                             group m by m.GetParameters()[0].ParameterType into g
                             let methodRange = from m in g
                                               let methodName = m.Name.Substring(0, 1).ToLower() + m.Name.Substring(1)
                                               let propertyName = methodName.Substring(0, methodName.Length - "Property".Length)
                                               select KeyValuePair.Create(propertyName, m)
                             select KeyValuePair.Create(g.Key, MethodIndex.Empty.AddRange(methodRange));
            var methods = from m in allMethods
                          where m.GetParameters().Length > 0
                          && !m.Name.EndsWith("Property")
                          group m by m.GetParameters()[0].ParameterType into g
                          let methodRange = from m in g
                                            let methodName = m.Name.Substring(0, 1).ToLower() + m.Name.Substring(1)
                                            select KeyValuePair.Create(methodName, m)
                          select KeyValuePair.Create(g.Key, MethodIndex.Empty.AddRange(methodRange));
            var mergedProperties = _properties.MergeWith(properties);
            var mergedMethods = _methods.MergeWith(methods);

            return new MethodSet(mergedProperties, mergedMethods);
        }

        public MethodSet AddMethodResolver(IMethodResolver resolver)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Consuming Methods
        public async Task<object> ComputePropertyAsync(object target, string name)
        {
            var methodInfo = _properties.GetOperation(target.GetType(), name);

            if (methodInfo != null)
            {
                var result = methodInfo.Invoke(null, new[] { target });
                await Task.CompletedTask;
                return result;
            }
            else
            {
                throw new ComputeException($"Target object doesn't have a property '{name}'");
            }
        }

        public Task<object> ComputeMethodAsync(object target, string name, IEnumerable<object> parameters)
        {
            throw new NotImplementedException();
        }
        #endregion

        private static void MergeDictionaries(
            IImmutableDictionary<Type, IImmutableList<MethodInfo>> original,
            IEnumerable<IGrouping<Type, MethodInfo>> addon)
        {

            throw new NotImplementedException();
        }
    }
}