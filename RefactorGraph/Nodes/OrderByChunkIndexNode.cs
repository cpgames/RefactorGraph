using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.OrderByChunkIndex)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class OrderByChunkIndexNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string COLLECTION_IN_PORT_NAME = "CollectionIn";
        public const string COLLECTION_OUT_PORT_NAME = "CollectionOut";
        #endregion

        #region Constructors
        public OrderByChunkIndexNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "OrderByChunkIndex";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(ChunkCollection), null,
                COLLECTION_IN_PORT_NAME, false, null, "Collection");

            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, false, typeof(ChunkCollection), null,
                COLLECTION_OUT_PORT_NAME, false, null, "Collection");

            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var collection = GetPortValue<ChunkCollection>(COLLECTION_IN_PORT_NAME);
            if (collection == null)
            {
                return;
            }
            collection?.Sort((a, b) =>
            {
                if (a.index < b.index)
                {
                    return -1;
                }
                if (a.index > b.index)
                {
                    return 1;
                }
                return 0;
            });
            SetPortValue(COLLECTION_OUT_PORT_NAME, collection);
            _success = true;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}