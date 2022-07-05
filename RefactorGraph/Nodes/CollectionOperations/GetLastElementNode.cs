using System;
using System.Collections;
using System.Linq;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetLastElement)]
    public class GetLastElementNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string COLLECTION_PORT_NAME = "Collection";
        public const string ELEMENT_PORT_NAME = "Element";
        private bool _success;
        #endregion

        #region Properties
        public override bool Success => _success;
        #endregion

        #region Constructors
        public GetLastElementNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddElementPort(ELEMENT_PORT_NAME, false);
            AddCollectionPort(COLLECTION_PORT_NAME, true);
        }

        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _success = false;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection != null && collection.Count > 0)
            {
                OutputPropertyPorts.First(x => x.Name == ELEMENT_PORT_NAME).Value = collection[collection.Count - 1];
                _success = true;
            }
        }
        #endregion
    }
}