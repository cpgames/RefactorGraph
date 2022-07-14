using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.Partition)]
    public class PartitionNode : VariableNode<Partition>
    {
        #region Properties
        protected override bool SerializeValue => false;
        #endregion

        #region Constructors
        public PartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}