using System;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.EnumerateProjects)]
    public class EnumerateProjectsNode : RefactorNodeBase
    {
        #region Fields
        public const string PROJECT_FILTER_PORT_NAME = "ProjectFilter";
        public const string LOOP_PROJECT_PORT_NAME = "LoopProject";
        public const string PROJECT_PORT_NAME = "Project";

        [NodePropertyPort(PROJECT_FILTER_PORT_NAME, true, typeof(string), null, true)]
        public string ProjectFilter;

        [NodePropertyPort(PROJECT_PORT_NAME, false, typeof(Project), null, false, Serialized = false)]
        public Project Project;
        #endregion

        #region Constructors
        public EnumerateProjectsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            CreateOutputFlowPort(LOOP_PROJECT_PORT_NAME);
        }

        protected override void OnExecute(Connector prevConnector)
        {
            ProjectFilter = GetPortValue(PROJECT_FILTER_PORT_NAME, ProjectFilter);
            var projects = Utils.GetProjectsInSolution(ProjectFilter);
            if (projects == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            foreach (var project in projects)
            {
                Project = project;
                ExecutionState = ExecutePort(LOOP_PROJECT_PORT_NAME);
                if (ExecutionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }
        }
        #endregion
    }
}