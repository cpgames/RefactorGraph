using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.JoinCollections)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class JoinCollectionsNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string COLLECTION_1_PORT_NAME = "Collection1";
        public const string COLLECTION_2_PORT_NAME = "Collection2";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(COLLECTION_1_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection Collection1;

        [NodePropertyPort(COLLECTION_2_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection Collection2;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(ChunkCollection), null, false)]
        public ChunkCollection Result;
        #endregion

        #region Constructors
        public JoinCollectionsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            var collection1 = GetPortValue<ChunkCollection>(COLLECTION_1_PORT_NAME);
            var collection2 = GetPortValue<ChunkCollection>(COLLECTION_2_PORT_NAME);
            if (collection1 != null && collection2 != null)
            {
                var result = new ChunkCollection();
                result.AddRange(collection1);
                result.AddRange(collection2);
                SetPortValue(RESULT_PORT_NAME, result);
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