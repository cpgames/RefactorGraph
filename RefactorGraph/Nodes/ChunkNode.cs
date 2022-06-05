using System;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.Chunk)]
    public class ChunkNode : VariableNode<Chunk>
    {
        #region Constructors
        public ChunkNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.Chunk) { }
        #endregion

        #region Methods
        public override void Reset()
        {
            Value = new Chunk();
        }
        #endregion
    }
}