using System;

namespace RefactorGraph
{
    [Flags]
    public enum PropertyModifier
    {
        None = 1,
        Static = 2,
        Abstract = 4,
        Virtual = 8,
    }
}