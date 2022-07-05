using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.GetCurrentDocument)]
    public class GetCurrentDocumentNode : RefactorNodeBase
    {
        #region Fields
        public const string DOCUMENT_PORT_NAME = "Document";

        [NodePropertyPort(DOCUMENT_PORT_NAME, false, typeof(TextDocument), null, false, Serialized = false)]
        public TextDocument Document;
        #endregion

        #region Constructors
        public GetCurrentDocumentNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            Document = Utils.GetActiveDocument();
            if (Document == null)
            {
                ExecutionState = ExecutionState.Failed;
            }
        }
        #endregion
    }
}