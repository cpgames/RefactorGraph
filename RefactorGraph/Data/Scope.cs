using System;

namespace RefactorGraph
{
    [Flags]
    public enum Scope
    {
        Scopeless = 1,
        Public = 2,
        Private = 4,
        Protected = 8,
        Internal = 16,
    }
}