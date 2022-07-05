using System;
using System.Linq;
using EnvDTE;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.DTE, RefactorNodeType.EnumerateDocumentsInSolution)]
    public class EnumerateDocumentsInSolutionNode : RefactorNodeBase
    {
        #region Fields
        public const string PROJECT_FILTER_PORT_NAME = "ProjectFilter";
        public const string FILENAME_FILTER_PORT_NAME = "FilenameFilter";
        public const string LOOP_DOCUMENT_PORT_NAME = "LoopDocument";
        public const string DOCUMENT_PORT_NAME = "Document";

        [NodePropertyPort(PROJECT_FILTER_PORT_NAME, true, typeof(string), null, true)]
        public string ProjectFilter;

        [NodePropertyPort(FILENAME_FILTER_PORT_NAME, true, typeof(string), null, true)]
        public string FilenameFilter;

        [NodePropertyPort(DOCUMENT_PORT_NAME, false, typeof(TextDocument), null, false, Serialized = false)]
        public TextDocument Document;
        #endregion

        #region Constructors
        public EnumerateDocumentsInSolutionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            CreateOutputFlowPort(LOOP_DOCUMENT_PORT_NAME);
        }

        protected override void OnExecute(Connector prevConnector)
        {
            ProjectFilter = GetPortValue(PROJECT_FILTER_PORT_NAME, ProjectFilter);
            FilenameFilter = GetPortValue(FILENAME_FILTER_PORT_NAME, FilenameFilter);
            var projectItems = Utils.GetProjectItemsInSolution(ProjectFilter, FilenameFilter);
            foreach (var projectItem in projectItems
                         .Where(projectItem => projectItem.Kind == Constants.vsProjectItemKindPhysicalFile))
            {
                if (!projectItem.IsOpen)
                {
                    projectItem.Open();
                }
                Document = projectItem.Document?.Object() as TextDocument;
                if (Document != null)
                {
                    var executionState = ExecutePort(LOOP_DOCUMENT_PORT_NAME);
                    if (executionState == ExecutionState.Failed)
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                }
            }
        }
        #endregion
    }
}