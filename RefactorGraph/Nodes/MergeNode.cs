using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.Merge)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class MergeNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string COLLECTION_PORT_NAME = "Collection";
        public const string RESULT_PORT_NAME = "Result";
        #endregion

        #region Constructors
        public MergeNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "Merge";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(ChunkCollection), null,
                COLLECTION_PORT_NAME, false, null, COLLECTION_PORT_NAME);

            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, false, typeof(Chunk), null,
                RESULT_PORT_NAME, false, null, RESULT_PORT_NAME);

            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var collection = GetPortValue(COLLECTION_PORT_NAME, new ChunkCollection());
            if (collection == null)
            {
                return;
            }
            var mergedChunk = new Chunk { content = string.Empty };
            foreach (var chunk in collection)
            {
                mergedChunk.content += chunk.content;
            }
            SetPortValue(RESULT_PORT_NAME, mergedChunk);
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