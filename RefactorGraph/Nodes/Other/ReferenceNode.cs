using System;
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
        public const string GRAPH_PORT_NAME = "GraphName";

        [NodePropertyPort(GRAPH_PORT_NAME, true, typeof(string), "", true)]
        public string GraphName;

        private FlowChart _referencedFlowChart;
        #endregion

        #region Constructors
        public ReferenceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            var graphNamePort = InputPropertyPorts.First(x => x.Name == GRAPH_PORT_NAME);
            graphNamePort.PropertyChanged += (sender,  args) =>
            {
                Header = string.IsNullOrEmpty(GraphName) ? "Reference" : GraphName;
            };
        }

        protected override void UpdatePorts()
        {
            AddElementPort(ARG_PORT_NAME, true);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            GraphName = GetPortValue(GRAPH_PORT_NAME, GraphName);
            if (_referencedFlowChart == null || _referencedFlowChart.Name != GraphName)
            {
                try
                {
                    Utils.Load(GraphName, out _referencedFlowChart);
                }
                catch (Exception e)
                {
                    NodeGraphManager.AddScreenLog(Owner, e.Message);
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }
            if (!Utils.ValidateGraph(_referencedFlowChart, out var startNode))
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }

            var arg =
                GetPortValue<object>(ARG_PORT_NAME) ??
                InputPropertyPorts.First(x => x.Name == "Arg").Value;
            if (arg != null)
            {
                var nodes = NodeGraphManager.FindNode(_referencedFlowChart, "Get Reference Arg");
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