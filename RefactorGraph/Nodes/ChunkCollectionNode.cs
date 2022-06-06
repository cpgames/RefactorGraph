using System;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.ChunkCollection)]
    public class ChunkCollectionNode : VariableNode<ChunkCollection>
    {
        #region Constructors
        public ChunkCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, "ChunkCollection") { }
        #endregion
    }
}