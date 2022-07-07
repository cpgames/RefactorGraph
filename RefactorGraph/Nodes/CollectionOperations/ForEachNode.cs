using System;
using System.Collections;
using System.Linq;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.ForEach)]
    public class ForEachNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string COMPLETED_PORT_NAME = "Done";
        public const string COLLECTION_PORT_NAME = "Collection";
        public const string ELEMENT_PORT_NAME = "Element";
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public ForEachNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
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
            if (collection == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            foreach (var item in collection)
            {
                OutputPropertyPorts.First(x => x.Name == ELEMENT_PORT_NAME).Value = item;
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = executionState;
                    return;
                }
            }
        }
        #endregion
    }
}