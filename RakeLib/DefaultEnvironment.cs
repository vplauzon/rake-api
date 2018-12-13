using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    internal static class DefaultEnvironment
    {
        public static IImmutableSet<string> PredefinedVariableNames
        {
            get => PredefinedVariables.Keys.ToImmutableSortedSet();
        }

        public static IImmutableDictionary<string, object> PredefinedVariables { get; } =
            ImmutableDictionary<string, object>.Empty;
    }
}