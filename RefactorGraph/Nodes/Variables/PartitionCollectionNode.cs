using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.PartitionCollection)]
    public class PartitionCollectionNode : VariableNode<List<Partition>>
    {
        #region Properties
        protected override bool SerializeValue => false;
        #endregion

        #region Constructors
        public PartitionCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override List<Partition> CopyValue(List<Partition> value)
        {
            return value == null ? null : new List<Partition>(value);
        }
        #endregion
    }
}