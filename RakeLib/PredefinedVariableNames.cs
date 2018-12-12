using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RakeLib
{
    internal static class PredefinedVariableNames
    {
        public static IImmutableSet<string> List { get; } = ImmutableSortedSet<string>.Empty
            .Add("util")
            .Add("functions");
    }
}