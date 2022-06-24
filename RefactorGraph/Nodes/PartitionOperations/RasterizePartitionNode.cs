using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.RasterizePartition)]
    public class RasterizePartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string DATA_PORT_NAME = "Data";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Partition;
        #endregion

        #region Properties
        public override bool Success => Partition != null;
        #endregion

        #region Constructors
        public RasterizePartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition != null)
            {
                Partition.Rasterize();
            }
        }
        #endregion
    }
}