using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByVariableAssignment)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop", false)]
    public class PartitionByVariableAssignmentNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string SOURCE_PORT_NAME = "Source";

        public const string VARIABLE_DEFINITION_FILTER_PORT_NAME = "VariableDefinitionFilter";
        public const string VARIABLE_TYPE_FILTER_PORT_NAME = "VariableTypeFilterRegex";
        public const string VARIABLE_NAME_FILTER_PORT_NAME = "VariableNameFilterRegex";

        public const string VARIABLE_BLOCK_PORT_NAME = "VariableBlock";
        public const string VARIABLE_SCOPE_PORT_NAME = "VariableScope";
        public const string VARIABLE_QUALIFIER_PORT_NAME = "VariableQualifier";
        public const string VARIABLE_READONLY_PORT_NAME = "VariableReadonly";
        public const string VARIABLE_TYPE_PORT_NAME = "VariableType";
        public const string VARIABLE_NAME_PORT_NAME = "VariableName";
        public const string VARIABLE_VALUE_PORT_NAME = "VariableValue";
        public const string IS_DECLARATION_PORT_NAME = "IsDeclaration";
        public const string IS_ASSIGNMENT_PORT_NAME = "IsAssignment";

        private const string VARIABLE_DEF_REGEX =
            "^\\s*#.*(*SKIP)(*F)|" +
            "\\s*using\\s*(*SKIP)(*F)|" +
            "\\s*return\\s*(*SKIP)(*F)|" +
            "^\\s*\\K\\w[\\w.\\s]+\\s*=\\s*(\\((?:[^()]++|" +
            "(?-1))*\\))\\s*=>\\s*({(?:[^{}]++|(?-1))*});|" +
            "^\\s*\\K\\w[\\w.\\s]+\\s*=\\s*[\\w\"][\\s\\S]*?;|" +
            "^\\s*\\K\\w[\\w.\\s<>\\[\\]]+\\s*;";

        private const string VARIABLE_SCOPE_REGEX = @"\b(?:public|private|protected|internal)\b";
        private const string VARIABLE_QUALIFIER_REGEX = @"\b(?:static|const)\b";
        private const string VARIABLE_READONLY_REGEX = @"\b(?:readonly)\b";
        private const string VARIABLE_TYPE_REGEX = @"\w+(?=\s+\w+)";
        private const string VARIABLE_NAME_REGEX = @"[\w.]+(?=\s*[=;])";
        private const string EQUALITY_REGEX = @"\s*=\s*";
        private const string VARIABLE_VALUE_REGEX = @"\s*\K[\s\S]+(?=;)";

        // Inputs
        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(VARIABLE_DEFINITION_FILTER_PORT_NAME, true, typeof(VariableDefinition), VariableDefinition.Any, true)]
        public VariableDefinition VariableDefinitionFilter;

        [NodePropertyPort(VARIABLE_TYPE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string VariableTypeFilterRegex;

        [NodePropertyPort(VARIABLE_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string VariableNameFilterRegex;

        // Outputs
        [NodePropertyPort(VARIABLE_BLOCK_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition VariableBlock;

        [NodePropertyPort(VARIABLE_SCOPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition VariableScope;

        [NodePropertyPort(VARIABLE_QUALIFIER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition VariableQualifier;

        [NodePropertyPort(VARIABLE_READONLY_PORT_NAME, false, typeof(bool), false, false)]
        public bool VariableReadonly;

        [NodePropertyPort(VARIABLE_TYPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition VariableType;

        [NodePropertyPort(VARIABLE_NAME_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition VariableName;

        [NodePropertyPort(VARIABLE_VALUE_PORT_NAME, false, typeof(Partition), null, true)]
        public Partition VariableValue;

        [NodePropertyPort(IS_DECLARATION_PORT_NAME, false, typeof(bool), false, false)]
        public bool IsDeclaration;

        [NodePropertyPort(IS_ASSIGNMENT_PORT_NAME, false, typeof(bool), false, false)]
        public bool IsAssignment;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public PartitionByVariableAssignmentNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null && !Source.IsPartitioned)
            {
                PartitionVariableAssignment();
            }
        }

        private void PartitionVariableAssignment()
        {
            var variableDefs = Source.PartitionByAllRegexMatches(VARIABLE_DEF_REGEX, PcreOptions.MultiLine);
            foreach (var variableBlock in variableDefs)
            {
                VariableBlock = variableBlock;
                PartitionVariableContent();
                if (ApplyFilter())
                {
                    ExecutePort(LOOP_PORT_NAME);
                }
            }
        }

        private void PartitionVariableContent()
        {
            var cur = VariableBlock;
            VariableScope = cur.PartitionByFirstRegexMatch(VARIABLE_SCOPE_REGEX, PcreOptions.Singleline);
            if (VariableScope != null)
            {
                cur = VariableScope.next;
            }
            VariableQualifier = cur.PartitionByFirstRegexMatch(VARIABLE_QUALIFIER_REGEX, PcreOptions.Singleline);
            if (VariableQualifier != null)
            {
                cur = VariableQualifier.next;
            }
            var variableReadonly = cur.PartitionByFirstRegexMatch(VARIABLE_READONLY_REGEX, PcreOptions.Singleline);
            if (variableReadonly != null)
            {
                VariableReadonly = true;
                cur = variableReadonly.next;
            }
            VariableType = cur.PartitionByFirstRegexMatch(VARIABLE_TYPE_REGEX, PcreOptions.MultiLine);
            if (VariableType != null)
            {
                cur = VariableType.next;
            }
            IsDeclaration = VariableType != null;
            VariableName = cur.PartitionByFirstRegexMatch(VARIABLE_NAME_REGEX, PcreOptions.MultiLine);
            if (VariableName != null)
            {
                cur = VariableName.next;
            }
            var equality = cur.PartitionByFirstRegexMatch(EQUALITY_REGEX, PcreOptions.MultiLine);
            IsAssignment = equality != null;
            if (equality == null)
            {
                IsAssignment = false;
                return;
            }
            cur = equality.next;
            VariableValue = cur.PartitionByFirstRegexMatch(VARIABLE_VALUE_REGEX, PcreOptions.MultiLine);
        }

        private bool ApplyFilter()
        {
            VariableDefinitionFilter = GetPortValue(VARIABLE_DEFINITION_FILTER_PORT_NAME, VariableDefinitionFilter);
            switch (VariableDefinitionFilter)
            {
                case VariableDefinition.Declaration:
                    if (!IsDeclaration)
                    {
                        return false;
                    }
                    break;
                case VariableDefinition.Assignment:
                    if (!IsAssignment)
                    {
                        return false;
                    }
                    break;
                case VariableDefinition.DeclarationOrAssignment:
                    if (!IsDeclaration && !IsAssignment)
                    {
                        return false;
                    }
                    break;
                case VariableDefinition.DeclarationAndAssignment:
                    if (!IsDeclaration || !IsAssignment)
                    {
                        return false;
                    }
                    break;
            }

            VariableTypeFilterRegex = GetPortValue(VARIABLE_TYPE_FILTER_PORT_NAME, VariableTypeFilterRegex);
            if (!string.IsNullOrEmpty(VariableTypeFilterRegex))
            {
                if (!PcreRegex.IsMatch(VariableType.Data, VariableTypeFilterRegex))
                {
                    return false;
                }
            }

            VariableNameFilterRegex = GetPortValue(VARIABLE_NAME_FILTER_PORT_NAME, VariableNameFilterRegex);
            if (!string.IsNullOrEmpty(VariableNameFilterRegex))
            {
                if (!PcreRegex.IsMatch(VariableName.Data, VariableNameFilterRegex))
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(COMPLETED_PORT_NAME);
        }
        #endregion
    }
}