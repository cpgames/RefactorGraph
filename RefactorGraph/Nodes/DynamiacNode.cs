using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes
{
    public abstract class DynamicNodeBase : RefactorNodeBase
    {
        #region Properties
        protected abstract IList DynamicPorts { get; }
        protected virtual int MinDynamicPorts => 0;
        #endregion

        #region Constructors
        protected DynamicNodeBase(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            PropertyChanged += OnPropertyChanged;
        }
        #endregion

        #region Methods
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ViewModel")
            {
                ViewModel.NodeViewChanged += view =>
                {
                    (view as DynamicNodeView).portAdded += AddDynamicPort;
                    (view as DynamicNodeView).portRemoved += RemoveDynamicPort;
                };
            }
        }
        
        public override void OnPreDestroy()
        {
            if (ViewModel.View is DynamicNodeView busView)
            {
                busView.portAdded -= AddDynamicPort;
                busView.portRemoved -= RemoveDynamicPort;
            }
            base.OnPreDestroy();
        }

        protected abstract void CreateDynamicPort(Guid guid);

        protected void AddDynamicPort(object sender, RoutedEventArgs routedEventArgs)
        {
            var guid = Guid.NewGuid();
            CreateDynamicPort(guid);
        }

        protected void RemoveDynamicPort(object sender, RoutedEventArgs routedEventArgs)
        {
            if (DynamicPorts.Count <= MinDynamicPorts)
            {
                return;
            }
            var lastPort = (NodePort)DynamicPorts[DynamicPorts.Count - 1];
            NodeGraphManager.DestroyNodePort(lastPort.Guid);
        }
        #endregion
    }
}