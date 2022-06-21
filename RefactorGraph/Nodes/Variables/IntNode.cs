using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.Int)]
    public class IntNode : VariableNode<int>
    {
        #region Properties
        protected override bool HasEditor => true;
        #endregion

        #region Constructors
        public IntNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}