using System;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.Chunk)]
    public class ChunkNode : VariableNode<Chunk>
    {
        #region Constructors
        public ChunkNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}