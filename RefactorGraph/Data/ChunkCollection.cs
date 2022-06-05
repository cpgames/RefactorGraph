using System.Collections.Generic;

namespace RefactorGraphdCore.Data
{
    public class ChunkCollection : List<Chunk>
    {
        public void Add(ChunkCollection collection)
        {
            AddRange(collection);
        }
    }

}