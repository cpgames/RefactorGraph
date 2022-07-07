using System;
using System.Collections;
using System.Linq;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetElement)]
    public class GetElementNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string INDEX_PORT_NAME = "Index";
        public const string COLLECTION_PORT_NAME = "Collection";
        public const string ELEMENT_PORT_NAME = "Element";

        [NodePropertyPort(INDEX_PORT_NAME, true, typeof(int), 0, true)]
        public int Index;
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

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            Index = GetPortValue(INDEX_PORT_NAME, Index);
            if (collection == null || Index >= collection.Count || Index < 0)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            OutputPropertyPorts.First(x => x.Name == ELEMENT_PORT_NAME).Value = collection[Index];
        }
        #endregion
    }
}