using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.ForEach)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop", false)]
    public class ForEachNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string COLLECTION_PORT_NAME = "CollectionOperations";
        public const string ELEMENT_PORT_NAME = "Element";
        private bool _success;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        public override bool Success => _success;
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

        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _success = false;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var collection = GetPortValue<IList>(COLLECTION_PORT_NAME);
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    SetPortValue(ELEMENT_PORT_NAME, item);
                    ExecutePort(LOOP_PORT_NAME);
                }
                _success = true;
            }
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            if (Success)
            {
                ExecutePort(COMPLETED_PORT_NAME);
            }
        }
        #endregion
    }
}