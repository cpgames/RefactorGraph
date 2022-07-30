using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.ReturnExecutionState)]
    public class ReturnExecutionStateNode : RefactorNodeBase
    {
        #region Fields
        public const string EXECUTION_STATE_PORT_NAME = "ReturnState";

        [NodePropertyPort(EXECUTION_STATE_PORT_NAME, false, typeof(ExecutionState), ExecutionState.Failed, true)]
        public ExecutionState ReturnState;
        #endregion

        #region Constructors
        public ReturnExecutionStateNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            ExecutionState = ReturnState;
        }
        #endregion
    }
}