using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetPureName)]
    public class GetPureNameNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string PURE_NAME_PORT_NAME = "PureName";

        public const string PURE_NAME_REGEX = @"[\s\S]*\.(*SKIP)(*F)|\w+";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(PURE_NAME_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition PureName;
        #endregion

        #region Constructors
        public GetPureNameNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            PureName = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            PureName = Partition.PartitionByFirstRegexMatch(Partition, PURE_NAME_REGEX);
        }
        #endregion
    }
}