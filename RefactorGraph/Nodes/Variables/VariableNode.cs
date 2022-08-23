using System;
using System.Linq;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    public abstract class VariableNode<T> : RefactorNodeBase
    {
        #region Fields
        public const string RESET_PORT_NAME = "ResetEveryRun";
        private T _value;
        private T _defaultValue;
        private bool _resetEveryRun;
        #endregion

        #region Properties
        protected virtual bool HasEditor => false;
        protected virtual Type ViewModelTypeOverride => null;
        protected virtual string DisplayNameOut => string.Empty;
        protected virtual bool SerializeValue => true;
        protected override bool AllowEditingHeaderOverride => true;
        protected override bool HasInput => false;
        protected override bool HasDone => false;
        public T Value
        {
            get
            {
                var port = InputPropertyPorts.FirstOrDefault(p => p.Name == "Value");
                NodeGraphManager.FindConnectedPorts(port, out var connectedPorts);
                if (!IsDeserializedFromXml && connectedPorts.Count > 0)
                {
                    return (T)(connectedPorts[0] as NodePropertyPort).Value;
                }
                return _value;
            }
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }
        #endregion

        #region Constructors
        protected VariableNode(Guid guid, FlowChart flowChart, string customHeader = null) : base(guid, flowChart)
        {
            Header = customHeader ?? typeof(T).Name;
        }
        #endregion

        #region Methods
        protected virtual T DefaultFactory()
        {
            return Activator.CreateInstance<T>();
        }

        protected virtual T CopyValue(T value)
        {
            return value;
        }

        public override void OnDeserialize()
        {
            base.OnDeserialize();
            if (!SerializeValue)
            {
                _value = DefaultFactory();
                _defaultValue = CopyValue(_value);
            }
            Utils.beginRefactor += OnBeginRefactor;
        }

        public override void OnCreate()
        {
            var type = typeof(T);

            _value = DefaultFactory();
            _defaultValue = CopyValue(_value);
            Utils.beginRefactor += OnBeginRefactor;

            NodeGraphManager.CreateNodePropertyPort(
                false, Guid.NewGuid(), this, true, type, _value, "Value", HasEditor, null, string.Empty,
                serializeValue: SerializeValue);
            NodeGraphManager.CreateNodePropertyPort(
                false, Guid.NewGuid(), this, true, typeof(bool), _resetEveryRun, RESET_PORT_NAME, true, null, "ResetEveryRun");
            NodeGraphManager.CreateNodePropertyPort(
                false, Guid.NewGuid(), this, false, type, _value, "Value", false, ViewModelTypeOverride, DisplayNameOut,
                serializeValue: SerializeValue);

            base.OnCreate();
        }

        public override void OnPreDestroy()
        {
            base.OnPreDestroy();
            Utils.beginRefactor -= OnBeginRefactor;
        }

        private void OnBeginRefactor()
        {
            _resetEveryRun = GetPortValue(RESET_PORT_NAME, _resetEveryRun);
            if (_resetEveryRun)
            {
                _value = CopyValue(_defaultValue);
            }
        }
        #endregion
    }
}