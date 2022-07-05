using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.GetCurrentProject)]
    public class GetCurrentProjectNode : RefactorNodeBase
    {
        #region Fields
        public const string PROJECT_PORT_NAME = "Project";

        [NodePropertyPort(PROJECT_PORT_NAME, false, typeof(Project), null, false, Serialized = false)]
        public Project Project;
        #endregion

        #region Constructors
        public GetCurrentProjectNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            Project = Utils.GetActiveProject();
            if (Project == null)
            {
                ExecutionState = ExecutionState.Failed;
            }
        }
        #endregion
    }
}