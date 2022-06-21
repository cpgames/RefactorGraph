using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetFirstElement)]
    public class GetFirstElementNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string COLLECTION_PORT_NAME = "CollectionOperations";
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

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection != null && collection.Count >  0)
            {
                SetPortValue(ELEMENT_PORT_NAME, collection[0]);
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