using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.GetCollectionSize)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class GetCollectionSizeNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string SIZE_PORT_NAME = "Size";
        public const string COLLECTION_PORT_NAME = "Collection";
        #endregion

        #region Constructors
        public GetCollectionSizeNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "GetCollectionSize";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(ChunkCollection), null,
                COLLECTION_PORT_NAME, false, null, COLLECTION_PORT_NAME);

            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, false, typeof(int), 0,
                SIZE_PORT_NAME, false, null, SIZE_PORT_NAME);

            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var collection = GetPortValue<ChunkCollection>(COLLECTION_PORT_NAME);
            if (collection != null)
            {
                SetPortValue(SIZE_PORT_NAME, collection.Count);
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