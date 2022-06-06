using System;
using System.Linq;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.Filter)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class FilterNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string CHUNK_NAME_PORT_NAME = "ChunkName";
        public const string CHUNK_PORT_NAME = "Chunk";
        public const string COLLECTION_IN_PORT_NAME = "CollectionIn";
        public const string COLLECTION_OUT_PORT_NAME = "CollectionOut";

        [NodePropertyPort(CHUNK_NAME_PORT_NAME, true, typeof(string), "MyChunk", true)]
        public string ChunkName;

        [NodePropertyPort(COLLECTION_IN_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection CollectionIn;

        [NodePropertyPort(COLLECTION_OUT_PORT_NAME, false, typeof(ChunkCollection), null, false)]
        public ChunkCollection CollectionOut;
        #endregion

        #region Constructors
        public FilterNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "Filter";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            var collectionIn = GetPortValue<ChunkCollection>(COLLECTION_IN_PORT_NAME);
            var collectionOut = new ChunkCollection();
            var chunkName = GetPortValue(CHUNK_PORT_NAME, ChunkName);
            if (collectionIn != null)
            {
                collectionOut.AddRange(collectionIn.Where(chunk => chunk.name == chunkName));
                _success = true;
            }
            SetPortValue(COLLECTION_OUT_PORT_NAME, collectionOut);
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}