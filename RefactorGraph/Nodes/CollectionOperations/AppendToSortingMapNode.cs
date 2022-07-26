using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Collections
{
    [Node]
    [RefactorNode(RefactorNodeGroup.CollectionOperations, RefactorNodeType.AppendToSortingMap)]
    public class AppendToSortingMapNode : RefactorNodeBase
    {
        #region Fields
        public const string MAP_PORT_NAME = "Map";
        public const string KEY_PORT_NAME = "Key";
        public const string VALUE_PORT_NAME = "Value";

        [NodePropertyPort(MAP_PORT_NAME, true, typeof(Dictionary<Partition, Partition>), null, false, Serialized = false)]
        public Dictionary<Partition, Partition> Map;

        [NodePropertyPort(KEY_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Key;

        [NodePropertyPort(VALUE_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Value;
        #endregion

        #region Constructors
        public AppendToSortingMapNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var map = GetPortValue<Dictionary<Partition, Partition>>(MAP_PORT_NAME);
            var key = GetPortValue<Partition>(KEY_PORT_NAME);
            var value = GetPortValue<Partition>(VALUE_PORT_NAME);
            if (map == null || key == null || value == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            map.Add(key, value);
        }
        #endregion
    }
}