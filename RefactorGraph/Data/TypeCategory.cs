using System;

namespace RefactorGraph
{
    [Flags]
    public enum TypeCategory
    {
        Class = 1,
        Interface = 2,
        Struct = 4,
        Enum = 8,
    }
}