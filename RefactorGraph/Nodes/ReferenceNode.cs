using System;
using System.Windows.Media;
using NodeGraph;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Custom, nodeType = RefactorNodeType.Reference)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class ReferenceNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string REFERENCE_PORT_NAME = "Reference";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        private FlowChart _referencedFlowChart;

        [NodePropertyPort(REFERENCE_PORT_NAME, true, typeof(string), "", true)]
        public string Reference;

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, true, DisplayName = "Source\n[Chunk]")]
        public Chunk Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(Chunk), null, true, DisplayName = "Result\n[Chunk]")]
        public Chunk Result;
        #endregion

        #region Constructors
        public ReferenceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        private Chunk GetDocument()
        {
            return Source;
        }

        private void SetDocument(Chunk chunk)
        {
            Result = chunk;
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            
            try
            {
                Utils.Load(Header, out _referencedFlowChart, Utils.GetCustomNodesPath());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            Result = null;
            if (Source != null &&
                Utils.ValidateGraph(_referencedFlowChart, out var getDocumentNode, out var setDocumentNode))
            {
                getDocumentNode.GetDocumentCallback += GetDocument;
                setDocumentNode.SetDocumentCallback += SetDocument;
                getDocumentNode.OnPreExecute(null);
                getDocumentNode.OnExecute(null);
                getDocumentNode.OnPostExecute(null);
                SetPortValue(RESULT_PORT_NAME, Result);
                _success = Result != null;
            }
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}