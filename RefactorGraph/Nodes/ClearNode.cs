using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.Clear)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class ClearNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string COLLECTION_PORT_NAME = "Collection";
        
        [NodePropertyPort(COLLECTION_PORT_NAME, true, typeof(ChunkCollection), null, false)]
        public ChunkCollection Collection;
        
        #endregion

        #region Constructors
        public ClearNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.Clear)
        {
            Header = "Clear";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            var collection = GetPortValue<ChunkCollection>(COLLECTION_PORT_NAME);
            if (collection != null)
            {
                collection.Clear();
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