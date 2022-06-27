using System;

namespace RefactorGraph
{
    [Flags]
    public enum ClassModifier
    {
        None = 1,
        Static = 2,
        Abstract = 4,
        Sealed = 8,
    }
}