using System;
using System.Linq;
using System.Windows;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node(ViewModelType = typeof(BusViewModel))]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.Bus)]
    public class BusNode : RefactorNodeBase
    {
        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public BusNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            var busView = ViewModel.View as BusView;
            busView.nodeAdded += AddOutputPort;
            busView.nodeRemoved += RemoveOutputPort;

            AddOutputPort(null, null);
            AddOutputPort(null, null);
        }

        public override void OnPreDestroy()
        {
            if (ViewModel.View is BusView busView)
            {
                busView.nodeAdded -= AddOutputPort;
                busView.nodeRemoved -= RemoveOutputPort;
            }
            base.OnPreDestroy();
        }

        private void AddOutputPort(object sender, RoutedEventArgs routedEventArgs)
        {
            var name = OutputFlowPorts.Count.ToString();
            var guid = Guid.NewGuid();
            NodeGraphManager.CreateNodeFlowPort(false, guid, this, false, null, name, name);
        }

        private void RemoveOutputPort(object sender, RoutedEventArgs routedEventArgs)
        {
            if (OutputFlowPorts.Count <= 2)
            {
                return;
            }
            var lastOutputPort = OutputFlowPorts.Last();
            NodeGraphManager.DestroyNodePort(lastOutputPort.Guid);
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