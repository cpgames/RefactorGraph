using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.StringCollection)]
    public class StringCollectionNode : VariableNode<List<string>>
    {
        #region Constructors
        public StringCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override List<string> CopyValue(List<string> value)
        {
            return value == null ? null : new List<string>(value);
        }
        #endregion
    }
}