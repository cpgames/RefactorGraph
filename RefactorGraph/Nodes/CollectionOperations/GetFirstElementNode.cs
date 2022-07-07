using System;
using System.Collections;
using System.Linq;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetFirstElement)]
    public class GetFirstElementNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string COLLECTION_PORT_NAME = "Collection";
        public const string ELEMENT_PORT_NAME = "Element";
        #endregion

        #region Constructors
        public GetFirstElementNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddElementPort(ELEMENT_PORT_NAME, false);
            AddCollectionPort(COLLECTION_PORT_NAME, true);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection == null || collection.Count == 0)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            OutputPropertyPorts.First(x => x.Name == ELEMENT_PORT_NAME).Value = collection[0];
        }
        #endregion
    }
}