using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.SetVariable)]
    public class SetVariableNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string VALUE_PORT_NAME = "Value";
        public const string VARIABLE_PORT_NAME = "Variable";
        #endregion

        #region Constructors
        public SetVariableNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddElementPort(VALUE_PORT_NAME, true);
            AddCollectionPort(VARIABLE_PORT_NAME, true);
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var value = GetPortValue<object>(VALUE_PORT_NAME);
            SetPortValue(VARIABLE_PORT_NAME, value);
        }
        #endregion
    }
}