using System;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.Int)]
    public class IntNode : VariableNode<int>
    {
        #region Properties
        protected override bool HasEditor => true;
        #endregion

        #region Constructors
        public IntNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.Int) { }
        #endregion
    }
}