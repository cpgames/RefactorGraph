using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.GetDocumentName)]
    public class GetDocumentNameNode : RefactorNodeBase
    {
        #region Fields
        public const string DOCUMENT_PORT_NAME = "Document";
        public const string NAME_PORT_NAME = "DocumentName";

        [NodePropertyPort(DOCUMENT_PORT_NAME, true, typeof(TextDocument), null, false, Serialized = false)]
        public TextDocument Document;

        [NodePropertyPort(NAME_PORT_NAME, false, typeof(string), "", false, Serialized = false)]
        public string DocumentName;
        #endregion

        #region Constructors
        public GetDocumentNameNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Document = GetPortValue<TextDocument>(DOCUMENT_PORT_NAME);
            if (Document == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            DocumentName = Document.Parent.Name;
        }
        #endregion
    }
}