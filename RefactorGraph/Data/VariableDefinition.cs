using System;

namespace RefactorGraph
{
    [Flags]
    public enum VariableDefinition
    {
        Declaration = 1,
        Assignment = 2
    }
}