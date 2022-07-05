using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.Document)]
    public class DocumentNode : VariableNode<Document>
    {
        #region Properties
        protected override bool HasEditor => false;
        #endregion

        #region Constructors
        public DocumentNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}