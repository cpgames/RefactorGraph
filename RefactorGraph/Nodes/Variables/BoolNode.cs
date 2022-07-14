using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.Bool)]
    public class BoolNode : VariableNode<bool>
    {
        #region Properties
        protected override bool HasEditor => true;
        #endregion

        #region Constructors
        public BoolNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}