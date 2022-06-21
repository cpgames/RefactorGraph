namespace RefactorGraph.Nodes
{
    public enum RefactorNodeType
    {
        // CollectionOperations
        AppendToCollection,
        ClearCollection,
        ForEach,
        GetCollectionSize,
        GetElement,
        GetFirstElement,
        GetLastElement,
        // LogicOperations
        Add,
        Compare,
        Equals,
        Multiply,
        Subtract,
        // Other
        Bus,
        ConvertToString,
        Print,
        Start,
        // PartitionOperations
        GetPartitionData,
        InsertAfter,
        InsertBefore,
        PartitionByFunctionCall,
        PartitionByFunction,
        PartitionIsValid,
        PartitionByAllRegexMatches,
        PartitionByFirstRegexMatch,
        PartitionByClasses,
        Reference,
        RemoveFunctionParameter,
        RemovePartition,
        RegexMatchPresentInPartition,
        SetPartitionData,
        SwapPartitions,
        // StringOperations
        RegexMatchPresentInString,
        StringFormat,
        StringRemove,
        StringReplace,
        Substring,
        // Variables
        IntCollection,
        Int,
        PartitionCollection,
        Partition,
        SetVariable,
        StringCollection,
        String
    }
}