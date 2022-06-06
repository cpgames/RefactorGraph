using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Document, nodeType = RefactorNodeType.SetDocument)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    public class SetDocumentNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string DOCUMENT_PORT_NAME = "Document";
        #endregion

        #region Properties
        public Action<Chunk> SetDocumentCallback { get;  set; }
        #endregion

        #region Constructors
        public SetDocumentNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(Chunk), null,
                DOCUMENT_PORT_NAME, false, null, DOCUMENT_PORT_NAME);
            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            if (SetDocumentCallback != null)
            {
                var documentChunk = GetPortValue<Chunk>(DOCUMENT_PORT_NAME);
                SetDocumentCallback(documentChunk);
                _success = true;
            }
        }
        #endregion
    }
}