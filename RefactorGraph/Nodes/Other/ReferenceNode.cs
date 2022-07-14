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
        private FlowChart _referencedFlowChart;
        #endregion

        #region Constructors
        public ReferenceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
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
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }
            if (!Utils.ValidateGraph(_referencedFlowChart, out var startNode))
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            ExecutionState = startNode.Execute(null);
        }
        #endregion
    }
}