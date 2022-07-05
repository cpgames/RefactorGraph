using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetNextPartition)]
    public class GetNextPartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string NEXT_PARTITION_PORT_NAME = "NextPartition";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(NEXT_PARTITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition NextPartition;
        #endregion

        #region Constructors
        public GetNextPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            NextPartition = null;
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
            NextPartition = Partition.next;
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Skipped;
            }
        }
        #endregion
    }
}