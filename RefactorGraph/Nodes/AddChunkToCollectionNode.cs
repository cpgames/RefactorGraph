using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.AddChunkToCollection)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class AddChunkToCollectionNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string CHUNK_PORT_NAME = "Chunk";
        public const string INSERT_AT_START_PORT_NAME = "InsertAtStart";
        public const string COLLECTION_PORT_NAME = "Collection";

        [NodePropertyPort(CHUNK_PORT_NAME, true, typeof(Chunk), null, false)]
        public Chunk Chunk;

        [NodePropertyPort(COLLECTION_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection Collection;

        [NodePropertyPort(INSERT_AT_START_PORT_NAME, true, typeof(bool), false, true)]
        public bool InsertAtStart;
        #endregion

        #region Constructors
        public AddChunkToCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            var collection = GetPortValue<ChunkCollection>(COLLECTION_PORT_NAME);
            var chunk = GetPortValue<Chunk>(CHUNK_PORT_NAME);
            if (chunk != null)
            {
                if (collection == null)
                {
                    collection = new ChunkCollection();
                }
                var insertAtStart = GetPortValue(INSERT_AT_START_PORT_NAME, InsertAtStart);
                if (insertAtStart)
                {
                    collection.Insert(0, chunk);
                }
                else
                {
                    collection.Add(chunk);
                }
                _success = true;
            }
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}