using System;
using System.Collections;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node(ViewModelType = typeof(DynamicNodeViewModel))]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.Bus)]
    public class BusNode : DynamicNodeBase
    {
        #region Fields
        public const string PROCEED_PORT_NAME = "ProceedOnFailure";

        [NodePropertyPort(PROCEED_PORT_NAME, true, typeof(bool), false, true)]
        public bool ProceedOnFailure;
        #endregion

        #region Properties
        protected override bool HasDone => false;
        protected override IList DynamicPorts => OutputFlowPorts;
        protected override int MinDynamicPorts => 2;
        #endregion

        #region Constructors
        public BusNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();

            AddDynamicPort(null, null);
            AddDynamicPort(null, null);
        }

        protected override void CreateDynamicPort(Guid guid)
        {
            var name = DynamicPorts.Count.ToString();
            NodeGraphManager.CreateNodeFlowPort(false, guid, this, false, null, name, name);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            ProceedOnFailure = GetPortValue(PROCEED_PORT_NAME, ProceedOnFailure);
        }

        protected override void OnPostExecute(Connector connector)
        {
            foreach (var outputPort in OutputFlowPorts)
            {
                foreach (var flowConnector in outputPort.Connectors)
                {
                    var executionState = flowConnector.Execute();
                    if (!ProceedOnFailure && executionState == ExecutionState.Failed)
                    {
                        ExecutionState = executionState;
                        return;
                    }
                }
            }
            base.OnPostExecute(connector);
        }
        #endregion
    }
}