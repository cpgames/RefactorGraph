using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.SwapPartitions)]
    public class SwapPartitionsNode : RefactorNodeBase
    {
        #region Fields
        public const string A_PORT_NAME = "PartitionA";
        public const string B_PORT_NAME = "PartitionB";

        [NodePropertyPort(A_PORT_NAME, true, typeof(Partition), null, true)]
        public Partition PartitionA;

        [NodePropertyPort(B_PORT_NAME, true, typeof(Partition), null, true)]
        public Partition PartitionB;
        #endregion

        #region Constructors
        public SwapPartitionsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            PartitionA = GetPortValue<Partition>(A_PORT_NAME);
            PartitionB = GetPortValue<Partition>(B_PORT_NAME);
            if (PartitionA != null &&
                PartitionB != null &&
                PartitionA != PartitionB &&
                !PartitionA.IsRoot &&
                !PartitionB.IsRoot)
            {
                var aPrev = PartitionA.prev;
                var aNext = PartitionA.next;

                PartitionA.prev = PartitionB.prev;
                PartitionA.prev.next = PartitionA;
                PartitionA.next = PartitionB.next;
                if (PartitionA.next != null)
                {
                    PartitionA.next.prev = PartitionA;
                }

                PartitionB.prev = aPrev;
                PartitionB.prev.next = PartitionB;
                PartitionB.next = aNext;
                if (PartitionB.next != null)
                {
                    PartitionB.next.prev = PartitionB;
                }

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