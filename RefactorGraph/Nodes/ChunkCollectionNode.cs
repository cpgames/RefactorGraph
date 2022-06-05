using System;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.ChunkCollection)]
    public class ChunkCollectionNode : VariableNode<ChunkCollection>
    {
        #region Constructors
        public ChunkCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.ChunkCollection, "ChunkCollection") { }
        #endregion

        #region Methods
        public override void Reset()
        {
            Value = new ChunkCollection();
        }
        #endregion
    }
}