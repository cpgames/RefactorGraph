using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.InsertBefore)]
    public class InsertBeforeNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string DATA_PORT_NAME = "Data";
        public const string NEW_PARTITION_PORT_NAME = "NewPartition";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, true, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(DATA_PORT_NAME, true, typeof(string), "", true)]
        public string Data;

        [NodePropertyPort(NEW_PARTITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition NewPartition;
        #endregion
        

        #region Constructors
        public InsertBeforeNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            NewPartition = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            Data = GetPortValue(DATA_PORT_NAME, Data);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            NewPartition = new Partition
            {
                data = Data,
                prev = Partition.prev,
                next = Partition
            };
            Partition.prev.next = NewPartition;
            Partition.prev = NewPartition;
        }
        #endregion
    }
}