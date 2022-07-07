using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetCollectionSize)]
    public class GetCollectionSizeNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string SIZE_PORT_NAME = "Size";
        public const string COLLECTION_PORT_NAME = "Collection";

        [NodePropertyPort(SIZE_PORT_NAME, false, typeof(int), 0, false)]
        public int Size;
        #endregion

        #region Constructors
        public GetCollectionSizeNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddCollectionPort(COLLECTION_PORT_NAME, true);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            Size = collection.Count;
        }
        #endregion
    }
}