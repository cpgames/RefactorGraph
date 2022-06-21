using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.IntCollection)]
    public class IntCollectionNode : VariableNode<List<int>>
    {
        #region Constructors
        public IntCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}