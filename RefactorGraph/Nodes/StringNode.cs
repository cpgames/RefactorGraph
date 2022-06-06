using System;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.String)]
    public class StringNode : VariableNode<string>
    {
        #region Properties
        protected override bool HasEditor => true;
        #endregion

        #region Constructors
        public StringNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }

        protected override string DefaultFactory()
        {
            return string.Empty;
        }
        #endregion
    }
}