using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes
{
    public abstract class RefactorNodeBase : Node
    {
        #region Fields
        private const string INPUT_PORT_NAME = "Input";
        private const string OUTPUT_PORT_NAME = "Output";
        #endregion

        #region Properties
        protected virtual bool AllowEditingHeaderOverride => false;

        protected virtual bool HasInput => true;
        protected virtual bool HasOutput => true;
        public virtual bool Success => true;
        #endregion

        #region Constructors
        protected RefactorNodeBase(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();

            var refactorNodeAtt = GetType().GetAttribute<RefactorNodeAttribute>();
            var nodeType = refactorNodeAtt.nodeType;
            var matches = Regex.Matches(nodeType.ToString(), "[A-Z][a-z]*");
            var header = matches.Cast<Match>().Aggregate(string.Empty, (current, match) => current + match.Value + " ");
            Header = header.Trim();
            HeaderBackgroundColor = NodeColors.brushes[refactorNodeAtt.nodeGroup];
            HeaderFontColor = Brushes.White;
            AllowEditingHeader = AllowEditingHeaderOverride;
            AllowCircularConnection = true;

            if (HasInput)
            {
                NodeGraphManager.CreateNodeFlowPort(false, Guid.NewGuid(), this, true, name: INPUT_PORT_NAME, displayName: "In");
            }
            if (HasOutput)
            {
                NodeGraphManager.CreateNodeFlowPort(false, Guid.NewGuid(), this, false, name: OUTPUT_PORT_NAME, displayName: "Out");
            }
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            if (HasOutput && Success)
            {
                ExecutePort(OUTPUT_PORT_NAME);
            }
            if (!Success)
            {
                ExecutionState = NodeExecutionState.Failed;
            }
        }

        protected TValue GetPortValue<TValue>(string portName, TValue defaultValue = default)
        {
            var port = NodeGraphManager.FindNodePropertyPort(this, portName);
            NodeGraphManager.FindConnectedPorts(port, out var connectedPorts);
            var otherPort = connectedPorts.Count > 0 ? connectedPorts[0] as NodePropertyPort : null;
            return otherPort != null ? (TValue)otherPort.Value : defaultValue;
        }

        protected void ExecutePort(string portName)
        {
            var port = OutputFlowPorts.FirstOrDefault(p => p.Name == portName);
            if (port == null)
            {
                NodeGraphManager.AddScreenLog(Owner, $"Port {portName} not found");
                return;
            }
            foreach (var connector in port.Connectors)
            {
                connector.OnPreExecute();
                connector.OnExecute();
                connector.OnPostExecute();
            }
        }
        #endregion
    }
}