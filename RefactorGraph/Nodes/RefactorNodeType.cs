﻿namespace RefactorGraph.Nodes
{
    public enum RefactorNodeType
    {
        // CollectionOperations
        AppendToCollection,
        AppendToSortingMap,
        ClearCollection,
        ForEach,
        GetCollectionSize,
        GetElement,
        GetFirstElement,
        GetLastElement,
        // DTE
        EnumerateDocumentsInProject,
        EnumerateDocumentsInSolution,
        EnumerateProjects,
        GetCurrentDocument,
        GetCurrentProject,
        GetDocumentName,
        GetDocumentPartition,
        GetProjectName,
        // LogicOperations
        Add,
        Compare,
        Multiply,
        Subtract,
        // Other
        Bus,
        Print,
        Reference,
        Start,
        // PartitionOperations
        FindFirstPartitionInCollection,
        GetNextPartition,
        GetPartitionData,
        GetPreviousPartition,
        InsertAfter,
        InsertBefore,
        PartitionByClass,
        PartitionByFirstRegexMatch,
        PartitionByFunctionCall,
        PartitionByFunction,
        PartitionByIfElse,
        PartitionByParameters,
        PartitionByRegexMatch,
        PartitionByVariable,
        PartitionIsValid,
        RegexMatchPresentInPartitionCollection,
        RegexMatchPresentInPartition,
        RemovePartition,
        SetPartitionData,
        SortPartitions,
        SwapPartitions,
        // StringOperations
        ConvertToString,
        RegexMatchPresentInString,
        StringFormat,
        StringRemove,
        StringReplace,
        StringToLowerFirstCharacter,
        StringToUpperFirstCharacter,
        Substring,
        // Variables
        BoolCollection,
        Bool,
        IntCollection,
        Int,
        PartitionCollection,
        Partition,
        PartitionSortingMap,
        StringCollection,
        String,
    }
}