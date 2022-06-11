using System;
using System.Windows.Media;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Logic, nodeType = RefactorNodeType.GetFirstElement)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class GetFirstElementNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string CHUNK_PORT_NAME = "Chunk";
        public const string COLLECTION_PORT_NAME = "Collection";

        [NodePropertyPort(COLLECTION_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection Collection;

        [NodePropertyPort(CHUNK_PORT_NAME, false, typeof(Chunk), null, false)]
        public Chunk Chunk;
        #endregion

        #region Constructors
        public GetFirstElementNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
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
            if (collection != null && collection.Count > 0)
            {
                SetPortValue(CHUNK_PORT_NAME, collection[0]);
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