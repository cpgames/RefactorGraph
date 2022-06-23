using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.RemoveParameter)]
    public class RemoveParameterNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, true)]
        public Partition Partition;
        #endregion

        #region Constructors
        public RemoveParameterNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition != null &&
                !Partition.IsRoot /* can't remove root */)
            {
                // remove comma
                if (!Partition.prev.IsRoot)
                {
                    Partition.prev.Remove();
                }
                else
                {
                    Partition.next?.Remove();
                }
                Partition.Remove();
            }
        }
        #endregion
    }
}