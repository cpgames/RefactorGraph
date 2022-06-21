using System;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
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
        protected virtual string DisplayNameOut => string.Empty;
        protected override bool AllowEditingHeaderOverride => true;
        protected override bool HasInput => false;
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        protected VariableNode(Guid guid, FlowChart flowChart, string customHeader = null) : base(guid, flowChart)
        {
            Header = customHeader ?? typeof(T).Name;
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
        protected virtual T DefaultFactory()
        {
            return Activator.CreateInstance<T>();
        }

        public override void OnCreate()
        {
            var type = typeof(T);
            _value = DefaultFactory();

            NodeGraphManager.CreateNodePropertyPort(
                false, Guid.NewGuid(), this, false, type, _value, "Value", HasEditor, ViewModelTypeOverride, DisplayNameOut);

            if (HasSetter)
            {
                NodeGraphManager.CreateNodePropertyPort(
                    false, Guid.NewGuid(), this, true, type, _value, "Value", false, null, string.Empty, true);
            }
            base.OnCreate();
        }
        #endregion
    }
}