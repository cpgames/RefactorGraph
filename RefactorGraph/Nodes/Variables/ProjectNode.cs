using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.Project)]
    public class ProjectNode : VariableNode<Project>
    {
        #region Properties
        protected override bool HasEditor => false;
        #endregion

        #region Constructors
        public ProjectNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}