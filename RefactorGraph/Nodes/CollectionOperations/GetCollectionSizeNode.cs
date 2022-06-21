using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.GetCollectionSize)]
    public class GetCollectionSizeNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string SIZE_PORT_NAME = "Size";
        public const string COLLECTION_PORT_NAME = "CollectionOperations";

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

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection != null)
            {
                SetPortValue(SIZE_PORT_NAME, collection.Count);
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