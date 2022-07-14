using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.BoolCollection)]
    public class BoolCollectionNode : VariableNode<List<bool>>
    {
        #region Constructors
        public BoolCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}