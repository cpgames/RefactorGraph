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
        public const string VARIABLE_MODIFIER_PORT_NAME = "VariableModifier";
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
        private const string VARIABLE_MODIFIER_REGEX = @"\b(?:static|const)\b";
        private const string VARIABLE_READONLY_REGEX = @"\b(?:readonly)\b";
        private const string VARIABLE_TYPE_REGEX = @"\w+(?=\s+\w+)";
        private const string VARIABLE_NAME_REGEX = @"[\w.]+(?=\s*[=;])";
        private const string EQUALITY_REGEX = @"\s*=\s*";
        private const string VARIABLE_VALUE_REGEX = @"\s*\K[\s\S]+(?=;)";

        // Inputs
        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(VARIABLE_DEFINITION_FILTER_PORT_NAME, true, typeof(VariableDefinition), VariableDefinition.Assignment | VariableDefinition.Declaration, true)]
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

        [NodePropertyPort(VARIABLE_MODIFIER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition VariableModifier;

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

        private bool _somethingReturned;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        public override bool Success => _somethingReturned;
        #endregion

        #region Constructors
        public PartitionByVariableAssignmentNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _somethingReturned = false;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Partition.IsValidAndNotPartitioned(Source))
            {
                PartitionVariableAssignment(Source);
            }
        }

        private void PartitionVariableAssignment(Partition cur)
        {
            var variableBlock = cur.PartitionByFirstRegexMatch(VARIABLE_DEF_REGEX, PcreOptions.MultiLine);
            if (variableBlock == null)
            {
                return;
            }
            var variableScope = variableBlock.PartitionByFirstRegexMatch(VARIABLE_SCOPE_REGEX, PcreOptions.Singleline);
            cur = variableScope != null ? variableScope.next : variableBlock;
            var variableModifier = cur.PartitionByFirstRegexMatch(VARIABLE_MODIFIER_REGEX, PcreOptions.Singleline);
            cur = variableModifier != null ? variableModifier.next : cur;
            var variableReadonlyPartition = cur.PartitionByFirstRegexMatch(VARIABLE_READONLY_REGEX, PcreOptions.Singleline);
            var variableReadonly = false;
            if (variableReadonlyPartition != null)
            {
                variableReadonly = true;
                cur = variableReadonlyPartition.next;
            }
            var variableType = cur.PartitionByFirstRegexMatch(VARIABLE_TYPE_REGEX, PcreOptions.MultiLine);
            var isDeclaration = false;
            if (variableType != null)
            {
                isDeclaration = true;
                cur = variableType.next;
            }
            var variableName = cur.PartitionByFirstRegexMatch(VARIABLE_NAME_REGEX, PcreOptions.MultiLine);
            cur = variableName.next;
            var equality = cur.PartitionByFirstRegexMatch(EQUALITY_REGEX, PcreOptions.MultiLine);
            var isAssignment = false;
            Partition variableValue = null;
            if (equality != null)
            {
                isAssignment = true;
                cur = equality.next;
                variableValue = cur.PartitionByFirstRegexMatch(VARIABLE_VALUE_REGEX, PcreOptions.MultiLine);
            }
            if (ApplyFilter(isDeclaration, isAssignment, variableType, variableName))
            {
                VariableBlock = variableBlock;
                VariableScope = variableScope;
                VariableModifier = variableModifier;
                VariableReadonly = variableReadonly;
                VariableType = variableType;
                VariableName = variableName;
                VariableValue = variableValue;
                IsDeclaration = isDeclaration;
                IsAssignment = isAssignment;
                ExecutePort(LOOP_PORT_NAME);
                _somethingReturned = true;
            }
            cur = variableBlock.next;
            if (cur != null)
            {
                PartitionVariableAssignment(cur);
            }
        }

        private bool ApplyFilter(bool isDeclaration, bool isAssignment, Partition variableType, Partition variableName)
        {
            VariableDefinitionFilter = GetPortValue(VARIABLE_DEFINITION_FILTER_PORT_NAME, VariableDefinitionFilter);
            if (isDeclaration && (VariableDefinitionFilter & VariableDefinition.Declaration) == 0)
            {
                return false;
            }
            if (isAssignment && (VariableDefinitionFilter & VariableDefinition.Assignment) == 0)
            {
                return false;
            }

            VariableTypeFilterRegex = GetPortValue(VARIABLE_TYPE_FILTER_PORT_NAME, VariableTypeFilterRegex);
            if (!string.IsNullOrEmpty(VariableTypeFilterRegex))
            {
                if (!PcreRegex.IsMatch(variableType.Data, VariableTypeFilterRegex))
                {
                    return false;
                }
            }

            VariableNameFilterRegex = GetPortValue(VARIABLE_NAME_FILTER_PORT_NAME, VariableNameFilterRegex);
            if (!string.IsNullOrEmpty(VariableNameFilterRegex))
            {
                if (!PcreRegex.IsMatch(variableName.Data, VariableNameFilterRegex))
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