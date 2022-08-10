using System;
using System.IO;
using System.Linq;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.Reference)]
    public class ReferenceNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string ARG_PORT_NAME = "Arg";
        public const string GRAPH_PATH_PORT_NAME = "GraphPath";
        public const string RELATIVE_TO_OWNER_PORT_NAME = "RelativeToOwner";
        

        [NodePropertyPort(GRAPH_PATH_PORT_NAME, true, typeof(string), "", true)]
        public string GraphPath;

        [NodePropertyPort(RELATIVE_TO_OWNER_PORT_NAME, true, typeof(bool), true, true)]
        public bool RelativeToOwner;

        public FlowChart referencedFlowChart;
        #endregion

        #region Constructors
        public ReferenceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            var graphNamePort = InputPropertyPorts.First(x => x.Name == GRAPH_PATH_PORT_NAME);
            graphNamePort.PropertyChanged += (sender,  args) =>
            {
                Header = string.IsNullOrEmpty(GraphPath) ? "Reference" : Path.GetFileNameWithoutExtension(GraphPath);
            };
        }

        protected override void UpdatePorts()
        {
            AddElementPort(ARG_PORT_NAME, true);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            GraphPath = GetPortValue(GRAPH_PATH_PORT_NAME, GraphPath);
            RelativeToOwner = GetPortValue(RELATIVE_TO_OWNER_PORT_NAME, RelativeToOwner);

            string filePath;
            if (RelativeToOwner)
            {
                if (!Utils.GetGraphPath(Owner.Guid, out var ownerFilePath))
                {
                    throw new Exception("No graph found.");
                }
                var folderPath = Path.GetDirectoryName(ownerFilePath);
                filePath = Path.Combine(folderPath, GraphPath);
            }
            else
            {
                filePath = Path.GetFullPath(GraphPath);
            }
            if (!filePath.EndsWith(".rgraph"))
            {
                filePath += ".rgraph";
            }
            
            if (referencedFlowChart == null || 
                !Utils.GetGraphPath(referencedFlowChart.Guid, out var currentFilePath) ||
                currentFilePath != filePath)
            {
                try
                {
                    Utils.Load(filePath, out referencedFlowChart);
                }
                catch (Exception e)
                {
                    NodeGraphManager.AddScreenLog(Owner, e.Message);
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }
            if (!Utils.ValidateGraph(referencedFlowChart, out var startNode))
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }

            var arg =
                GetPortValue<object>(ARG_PORT_NAME) ??
                InputPropertyPorts.First(x => x.Name == "Arg").Value;
            if (arg != null)
            {
                var nodes = NodeGraphManager.FindNode(referencedFlowChart, "Get Reference Arg");
                foreach (var node in nodes.OfType<GetReferenceArgNode>())
                {
                    node.value = arg;
                }
            }
            ExecutionState = startNode.Execute(null);
        }
        #endregion
    }
}