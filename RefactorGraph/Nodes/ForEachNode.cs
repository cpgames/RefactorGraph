using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Logic, nodeType = RefactorNodeType.ForEach)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop Body", false)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    public class ForEachNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string COLLECTION_PORT_NAME = "Collection";
        public const string ELEMENT_PORT_NAME = "Element";
        #endregion

        #region Constructors
        public ForEachNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.ForEach)
        {
            Header = "ForEach";
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
                ELEMENT_PORT_NAME, false, null, ELEMENT_PORT_NAME);

            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            var collection = GetPortValue<ChunkCollection>(COLLECTION_PORT_NAME);

            if (collection != null)
            {
                try
                {
                    foreach (var chunk in collection)
                    {
                        _success = true;
                        SetPortValue(ELEMENT_PORT_NAME, chunk);
                        ExecutePort(LOOP_PORT_NAME);
                    }
                }
                catch (Exception e)
                {
                    NodeGraphManager.AddScreenLog(Owner, e.Message);
                    _success = false;
                }
            }
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(COMPLETED_PORT_NAME);
        }
        #endregion
    }
}