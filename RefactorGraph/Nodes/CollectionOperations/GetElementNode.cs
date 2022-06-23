using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetElement)]
    public class GetElementNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string INDEX_PORT_NAME = "Index";
        public const string COLLECTION_PORT_NAME = "CollectionOperations";
        public const string ELEMENT_PORT_NAME = "Element";
        private bool _success;

        [NodePropertyPort(INDEX_PORT_NAME, true, typeof(int), 0, true)]
        public int Index;
        #endregion

        #region Properties
        public override bool Success => _success;
        #endregion

        #region Constructors
        public GetElementNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddElementPort(ELEMENT_PORT_NAME, false);
            AddCollectionPort(COLLECTION_PORT_NAME, true, 1);
        }

        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _success = false;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            Index = GetPortValue(INDEX_PORT_NAME, Index);
            if (collection != null && collection.Count > Index && Index >= 0)
            {
                SetPortValue(ELEMENT_PORT_NAME, collection[Index]);
                _success = true;
            }
        }
        #endregion
    }
}