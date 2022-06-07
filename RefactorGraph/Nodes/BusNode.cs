using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node(ViewModelType = typeof(BusViewModel))]
    [RefactorNode(group = RefactorNodeGroup.Logic, nodeType = RefactorNodeType.Bus)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    public class BusNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        private readonly Dictionary<string, Guid> outputPorts = new Dictionary<string, Guid>();
        #endregion

        #region Constructors
        public BusNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

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
            var busView = ViewModel.View as BusView;
            busView.nodeAdded -= AddOutputPort;
            busView.nodeRemoved -= RemoveOutputPort;
            base.OnPreDestroy();
        }

        #region Methods
        private void AddOutputPort(object sender, RoutedEventArgs routedEventArgs)
        {
            var name = outputPorts.Count.ToString();
            
            var guid = Guid.NewGuid();
            NodeGraphManager.CreateNodeFlowPort(false, guid, this, false, null, name, name);
            outputPorts.Add(name, guid);
            //var busView = ViewModel.View as BusView;
            //busView.OnApplyTemplate();
        }

        private void RemoveOutputPort(object sender, RoutedEventArgs routedEventArgs)
        {
            if (outputPorts.Count <= 2)
            {
                return;
            }

            var lastOutputPort = outputPorts.Last();
            NodeGraphManager.DestroyNodePort(lastOutputPort.Value);
            outputPorts.Remove(lastOutputPort.Key);
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            _success = true;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            foreach (var outputPort in outputPorts)
            {
                ExecutePort(outputPort.Key);
            }
        }
        #endregion
    }
}