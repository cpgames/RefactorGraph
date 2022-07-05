using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.GetProjectName)]
    public class GetProjectNameNode : RefactorNodeBase
    {
        #region Fields
        public const string PROJECT_PORT_NAME = "Project";
        public const string NAME_PORT_NAME = "ProjectName";

        [NodePropertyPort(PROJECT_PORT_NAME, true, typeof(Project), null, false, Serialized = false)]
        public Project Project;

        [NodePropertyPort(NAME_PORT_NAME, false, typeof(string), "", false, Serialized = false)]
        public string ProjectName;
        #endregion

        #region Constructors
        public GetProjectNameNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Project = GetPortValue<Project>(PROJECT_PORT_NAME);
            if (Project == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            ProjectName = Project.Name;
        }
        #endregion
    }
}