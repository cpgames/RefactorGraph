using System;

namespace RefactorGraph
{
    [Flags]
    public enum FunctionKind
    {
        Constructor = 1,
        Regular = 2,
    }
}