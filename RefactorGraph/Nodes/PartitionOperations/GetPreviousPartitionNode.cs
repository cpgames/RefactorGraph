using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetPreviousPartition)]
    public class GetPreviousPartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string PREVIOUS_PARTITION_PORT_NAME = "PreviousPartition";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(PREVIOUS_PARTITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition PreviousPartition;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public GetPreviousPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            PreviousPartition = null;
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
            PreviousPartition = Partition.prev;
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Skipped;
            }
        }
        #endregion
    }
}