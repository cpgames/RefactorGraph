using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.PartitionSortingMap)]
    public class PartitionSortingMapNode : VariableNode<Dictionary<Partition, Partition>>
    {
        #region Properties
        protected override bool SerializeValue => false;
        #endregion

        #region Constructors
        public PartitionSortingMapNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override Dictionary<Partition, Partition> CopyValue(Dictionary<Partition, Partition> value)
        {
            return value == null ? null : new Dictionary<Partition, Partition>(value);
        }
        #endregion
    }
}