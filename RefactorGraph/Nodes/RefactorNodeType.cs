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
        FindFirstPartitionInCollection,
        GetNextPartition,
        GetPreviousPartition,
        GetPartitionData,
        InsertAfter,
        InsertBefore,
        PartitionByFunctionCall,
        PartitionByFunction,
        PartitionByIfElse,
        PartitionByVariableAssignment,
        PartitionIsValid,
        PartitionByAllRegexMatches,
        PartitionByFirstRegexMatch,
        PartitionByClass,
        RasterizePartition,
        Reference,
        RemoveFunctionCall,
        RemoveParameter,
        RemovePartition,
        RegexMatchPresentInPartitionCollection,
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
        StringCollection,
        String
    }
}