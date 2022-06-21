using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.ClearCollection)]
    public class ClearCollectionNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string COLLECTION_PORT_NAME = "Collection";
        #endregion

        #region Constructors
        public ClearCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddCollectionPort(COLLECTION_PORT_NAME, true);
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection != null)
            {
                collection.Clear();
                _success = true;
            }
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}