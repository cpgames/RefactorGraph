using System;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    public class RefactorNodeBase : Node
    {
        #region Fields
        private RefactorNodeType _nodeType;
        protected bool _success = true;
        #endregion

        #region Properties
        public RefactorNodeType NodeType
        {
            get => _nodeType;
            set
            {
                if (value != _nodeType)
                {
                    _nodeType = value;
                    RaisePropertyChanged("NodeType");
                }
            }
        }
        #endregion

        #region Constructors
        public RefactorNodeBase(Guid guid, FlowChart flowChart, RefactorNodeType nodeType) : base(guid, flowChart)
        {
            Header = nodeType.ToString();
            _nodeType = nodeType;
            var raiseEvent = NodeCreatedEvent;
            raiseEvent?.Invoke(this, null);
            AllowCircularConnection = true;
        }
        #endregion

        #region Methods
        public static event EventHandler NodeCreatedEvent;

        public virtual void Reset() { }

        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _success = false;
        }

        protected TValue GetPortValue<TValue>(string portName, TValue defaultValue = default)
        {
            var port = NodeGraphManager.FindNodePropertyPort(this, portName);
            NodeGraphManager.FindConnectedPorts(port, out var connectedPorts);
            var otherPort = connectedPorts.Count > 0 ? connectedPorts[0] as NodePropertyPort : null;
            return otherPort != null ? (TValue)otherPort.Value : defaultValue;
        }

        protected void SetPortValue(string portName, object value)
        {
            var port = NodeGraphManager.FindNodePropertyPort(this, portName);
            NodeGraphManager.FindConnectedPorts(port, out var connectedPorts);
            foreach (var connectedPort in connectedPorts)
            {
                if (connectedPort.Owner is IVariableNode variableNode)
                {
                    variableNode.Value = value;
                }
            }
            port.Value = value;
        }

        protected void ExecutePort(string portName)
        {
            if (_success)
            {
                var port = NodeGraphManager.FindNodeFlowPort(this, portName);
                foreach (var connector in port.Connectors)
                {
                    connector.OnPreExecute();
                    connector.OnExecute();
                    connector.OnPostExecute();
                }
            }
            else
            {
                ExecutionState = NodeExecutionState.Failed;
            }
        }
        #endregion
    }
}