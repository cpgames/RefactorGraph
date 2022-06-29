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
        #region Properties
        protected override bool HasOutput => false;
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

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            foreach (var outputPort in OutputFlowPorts)
            {
                foreach (var flowConnector in outputPort.Connectors)
                {
                    flowConnector.OnPreExecute();
                    flowConnector.OnExecute();
                    flowConnector.OnPostExecute();
                }
            }
        }
        #endregion
    }
}