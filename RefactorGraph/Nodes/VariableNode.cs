using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    public interface IVariableNode
    {
        #region Properties
        object Value { get; set; }
        #endregion
    }

    [Node]
    public abstract class VariableNode<T> : RefactorNodeBase, IVariableNode
    {
        #region Fields
        private object _value;
        #endregion

        #region Properties
        protected virtual bool HasSetter => true;
        protected virtual bool HasEditor => false;
        protected virtual Type ViewModelTypeOverride => null;
        #endregion

        #region Constructors
        protected VariableNode(Guid guid, FlowChart flowChart, RefactorNodeType nodeType, string customHeader = null) : base(guid, flowChart,  nodeType)
        {
            if (customHeader == null)
            {
                Header = typeof(T).Name;
            }
            else
            {
                Header = customHeader;
            }
            HeaderBackgroundColor = Brushes.DarkRed;
            HeaderFontColor = Brushes.White;
        }
        #endregion

        #region IVariableNode Members
        public object Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            var type = typeof(T);
            _value = Activator.CreateInstance(type);

            var port = NodeGraphManager.CreateNodePropertyPort(
                false, Guid.NewGuid(), this, false, type, _value, "Value", HasEditor, ViewModelTypeOverride);

            port.DynamicPropertyPortValueChanged += ValuePort_PropertyPortValueChanged;

            if (HasSetter)
            {
                NodeGraphManager.CreateNodePropertyPort(
                    false, Guid.NewGuid(), this, true, type, _value, "Value", false, null, string.Empty, true);
            }
            base.OnCreate();
        }

        private void ValuePort_PropertyPortValueChanged(NodePropertyPort port, object prevValue, object newValue)
        {
            Value = (Pattern)port.Value;
        }
        #endregion
    }
}