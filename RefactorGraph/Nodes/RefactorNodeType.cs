namespace RefactorGraph
{
    public enum RefactorNodeType
    {
        GetDocument,
        SetDocument,
        SplitRegex,
        SplitIndex,
        SetChunkData,
        CreateChunk,
        ReplaceRegex,
        AddChunkToCollection,
        JoinCollections,
        Merge,
        ForEach,
        Chunk,
        ChunkCollection,
        OrderByChunkIndex,
        GetElement,
        GetFirstElement,
        GetLastElement,
        GetCollectionSize,
        Equals,
        Int,
        String,
        Add,
        IntToChunk,
        Set,
        Bus,
        Filter,
        Clear,
        OrderAlphabetical,

        FindFunctionCalls,
        ParseFunction,
        Reference
    }
}