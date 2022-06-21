using System;
using System.Collections.Generic;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes
{
    public enum CollectionType
    {
        String,
        Int,
        Partition,
        Bool
    }

    public abstract class DynamicRefactorNodeBase : RefactorNodeBase
    {
        #region Fields
        public const string ELEMENT_TYPE_PORT_NAME = "ElementType";

        private CollectionType _elementType;
        #endregion

        #region Properties
        [NodePropertyPort(ELEMENT_TYPE_PORT_NAME, true, typeof(CollectionType), CollectionType.Partition, true)]
        public CollectionType ElementType
        {
            get => _elementType;
            set
            {
                if (_elementType != value)
                {
                    _elementType = value;
                    if (!IsDeserializedFromXml)
                    {
                        UpdatePorts();
                    }
                }
            }
        }
        #endregion

        #region Constructors
        protected DynamicRefactorNodeBase(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();

            UpdatePorts();
        }

        protected virtual void UpdatePorts() { }

        private void RemoveOldPort(string portName)
        {
            var port = NodeGraphManager.FindNodePropertyPort(this, portName);
            if (port != null)
            {
                NodeGraphManager.DestroyNodePort(port.Guid);
            }
        }

        protected void AddElementPort(string portName, bool input, int index = -1)
        {
            RemoveOldPort(portName);
            Type elementType;
            object defaultValue;
            switch (ElementType)
            {
                case CollectionType.String:
                    elementType = typeof(string);
                    defaultValue = "";
                    break;
                case CollectionType.Int:
                    elementType = typeof(int);
                    defaultValue = 0;
                    break;
                case CollectionType.Partition:
                    elementType = typeof(Partition);
                    defaultValue = null;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, input, elementType,
                defaultValue, portName, false, displayName: $"{portName}\n[{ElementType}]", index: index);
        }

        protected void AddCollectionPort(string portName, bool input, int index = -1)
        {
            RemoveOldPort(portName);
            Type collectionType;
            switch (ElementType)
            {
                case CollectionType.String:
                    collectionType = typeof(List<string>);
                    break;
                case CollectionType.Int:
                    collectionType = typeof(List<int>);
                    break;
                case CollectionType.Partition:
                    collectionType = typeof(List<Partition>);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, input, collectionType,
                null, portName, false, displayName: $"{portName}\n[List<{ElementType}>]", index: index);
        }
        #endregion
    }
}