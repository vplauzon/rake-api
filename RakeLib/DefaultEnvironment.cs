using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    public static class DefaultEnvironment
    {
        public static IImmutableDictionary<string, object> PredefinedVariables =
            ImmutableDictionary<string, object>.Empty;
    }
}