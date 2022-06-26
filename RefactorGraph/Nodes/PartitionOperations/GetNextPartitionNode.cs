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
        public const string NEXT_PORT_NAME = "Next";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Partition;

        [NodePropertyPort(NEXT_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Next;
        #endregion

        #region Properties
        public override bool Success => Next != null;
        #endregion

        #region Constructors
        public GetNextPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Next = null;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition != null)
            {
                Next = Partition.next;
            }
        }
        #endregion
    }
}