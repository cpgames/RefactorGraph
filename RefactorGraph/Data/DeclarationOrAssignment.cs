using System;

namespace RefactorGraph
{
    [Flags]
    public enum DeclarationOrAssignment
    {
        Declaration = 1,
        Assignment = 2
    }
}