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

        #region Methods
        protected override List<bool> CopyValue(List<bool> value)
        {
            return value == null ? null : new List<bool>(value);
        }
        #endregion
    }
}