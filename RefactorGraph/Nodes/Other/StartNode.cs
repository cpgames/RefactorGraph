using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.Start)]
    public class StartNode : RefactorNodeBase
    {
        #region Properties
        protected override bool HasInput => false;
        #endregion

        #region Constructors
        public StartNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}