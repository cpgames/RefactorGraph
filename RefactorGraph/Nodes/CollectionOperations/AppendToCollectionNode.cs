using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.AppendToCollection)]
    public class AppendToCollectionNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string COLLECTION_PORT_NAME = "CollectionOperations";
        public const string ELEMENT_PORT_NAME = "Element";
        
        #endregion

        #region Constructors
        public AppendToCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddCollectionPort(COLLECTION_PORT_NAME, true);
            AddElementPort(ELEMENT_PORT_NAME, true);
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            var element = GetPortValue<object>(ELEMENT_PORT_NAME);
            if (collection != null && element != null)
            {
                collection.Add(element);
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