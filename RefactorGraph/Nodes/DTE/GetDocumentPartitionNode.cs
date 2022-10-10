using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.GetDocumentPartition)]
    public class GetDocumentPartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string DOCUMENT_PORT_NAME = "Document";
        public const string PARTITION_PORT_NAME = "Partition";
        public const string SELECTION_PORT_NAME = "Selection";

        [NodePropertyPort(DOCUMENT_PORT_NAME, true, typeof(TextDocument), null, false, Serialized = false)]
        public TextDocument Document;

        [NodePropertyPort(PARTITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(SELECTION_PORT_NAME, false, typeof(bool), false, true)]
        public bool Selection;
        #endregion

        #region Constructors
        public GetDocumentPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
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
            Selection = GetPortValue(SELECTION_PORT_NAME, Selection);
            Partition = Selection && !Utils.IsSelectionEmpty(Document) ?
                Utils.GetSelectionPartition(Document) :
                Utils.GetDocumentPartition(Document);
        }

        protected override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            if (ExecutionState == ExecutionState.Executed)
            {
                Partition.Rasterize();
                if (Selection && !Utils.IsSelectionEmpty(Document))
                {
                    ExecutionState = Utils.SetSelectionPartition(Document, Partition) ?
                        ExecutionState.Executed :
                        ExecutionState.Skipped;
                }
                else
                {
                    ExecutionState = Utils.SetDocumentPartition(Document, Partition) ?
                        ExecutionState.Executed :
                        ExecutionState.Skipped;
                }
            }
        }
        #endregion
    }
}