using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    internal static class DefaultEnvironment
    {
        public static IImmutableSet<string> PredefinedVariables { get; } = ImmutableSortedSet<string>.Empty
            .Add("util")
            .Add("functions");
    }
}