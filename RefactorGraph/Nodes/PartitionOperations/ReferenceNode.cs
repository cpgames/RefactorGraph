using System;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.Reference)]
    public class ReferenceNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";

        private FlowChart _referencedFlowChart;

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, true)]
        public Partition Source;
        #endregion

        #region Constructors
        public ReferenceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            if (_referencedFlowChart == null)
            {
                try
                {
                    Utils.Load(Header, out _referencedFlowChart);
                }
                catch (Exception e)
                {
                    NodeGraphManager.AddScreenLog(Owner, e.Message);
                    return;
                }
            }
            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null &&
                Utils.ValidateGraph(_referencedFlowChart, out var startNode))
            {
                startNode.Result = Source;
                startNode.OnPreExecute(null);
                startNode.OnExecute(null);
                startNode.OnPostExecute(null);
            }
        }
        #endregion
    }
}