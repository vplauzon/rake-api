using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RakeLib
{
    public interface IMethodResolver
    {
        Type TargetType { get; }

        Task<object> ComputePropertyAsync(object target, string name);

        Task<object> ComputeMethodAsync(object target, string name, IEnumerable<object> parameters);
    }
}