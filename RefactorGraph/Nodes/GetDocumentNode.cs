using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Document, nodeType = RefactorNodeType.GetDocument)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class GetDocumentNode : RefactorNodeBase
    {
        #region Fields
        public const string OUTPUT_PORT_NAME = "Output";
        public const string DOCUMENT_PORT_NAME = "Document";

        private Chunk _documentChunk;
        #endregion

        #region Properties
        public Func<Chunk> GetDocumentCallback { get; set; }
        #endregion

        #region Constructors
        public GetDocumentNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, false, typeof(Chunk), null,
                DOCUMENT_PORT_NAME, false, null, DOCUMENT_PORT_NAME);

            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            if (GetDocumentCallback == null)
            {
                return;
            }

            _documentChunk = GetDocumentCallback();
            _success = _documentChunk != null;
            SetPortValue(DOCUMENT_PORT_NAME, _documentChunk);
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}