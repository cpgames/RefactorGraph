﻿using System;
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
        public const string PROJECT_PORT_NAME = "Project";

        [NodePropertyPort(PROJECT_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ProjectFilter;

        [NodePropertyPort(PROJECT_PORT_NAME, false, typeof(Project), null, false, Serialized = false)]
        public Project Project;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public EnumerateProjectsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
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
                ExecutionState = ExecutePort(LOOP_PORT_NAME);
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