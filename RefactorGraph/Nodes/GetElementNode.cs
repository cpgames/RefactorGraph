using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Logic, nodeType = RefactorNodeType.GetElement)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class GetElementNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string CHUNK_PORT_NAME = "Chunk";
        public const string INDEX_PORT_NAME = "Index";
        public const string COLLECTION_PORT_NAME = "Collection";

        [NodePropertyPort(COLLECTION_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection Collection;

        [NodePropertyPort(INDEX_PORT_NAME, true, typeof(int), 0, true)]
        public int Index;

        [NodePropertyPort(CHUNK_PORT_NAME, false, typeof(Chunk), null, false)]
        public Chunk Chunk;
        #endregion

        #region Constructors
        public GetElementNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "GetElement";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var collection = GetPortValue<ChunkCollection>(COLLECTION_PORT_NAME);
            var index = GetPortValue(INDEX_PORT_NAME, Index);
            if (collection != null)
            {
                if (collection.Count > index)
                {
                    SetPortValue(CHUNK_PORT_NAME, collection[index]);
                    _success = true;
                }
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