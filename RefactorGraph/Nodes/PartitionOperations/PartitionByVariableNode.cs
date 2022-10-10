using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByVariable)]
    public class PartitionByVariableNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        public const string DECLARATION_OR_ASSIGNMENT_FILTER_PORT_NAME = "DeclarationOrAssignmentFilter";
        public const string VARIABLE_TYPE_FILTER_PORT_NAME = "VariableTypeFilter";
        public const string VARIABLE_NAME_FILTER_PORT_NAME = "VariableNameFilter";
        public const string VARIABLE_VALUE_FILTER_PORT_NAME = "VariableValueFilter";

        public const string VARIABLE_BLOCK_PORT_NAME = "VariableBlock";
        public const string VARIABLE_SCOPE_PORT_NAME = "VariableScope";
        public const string VARIABLE_MODIFIER_PORT_NAME = "VariableModifier";
        public const string VARIABLE_READONLY_PORT_NAME = "VariableReadonly";
        public const string VARIABLE_TYPE_PORT_NAME = "VariableType";
        public const string VARIABLE_NAME_PORT_NAME = "VariableName";
        public const string VARIABLE_VALUE_PORT_NAME = "VariableValue";
        public const string IS_DECLARATION_PORT_NAME = "IsDeclaration";
        public const string IS_ASSIGNMENT_PORT_NAME = "IsAssignment";

        private const string DECLARATION_REGEX = @"^\s*\K\w[\w.\s<>\[\]\??]+\s*";
        private const string VARIABLE_REGEX =
            "^\\s*#.*(*SKIP)(*F)|" + // skip regions
            "\\s*using\\s*(*SKIP)(*F)|" + // skip usings
            "\\s*return\\s*(*SKIP)(*F)|" + // skip returns
            DECLARATION_REGEX + "=\\s*(\\((?:[^()]++|(?-1))*\\))\\s*=>\\s*({(?:[^{}]++|(?-1))*});|" + // assignment with lambda and/or declaration
            DECLARATION_REGEX + "=\\s*[\\w\"][\\s\\S]*?;|" + // standard assignment and/or declaration and 
            DECLARATION_REGEX + ";"; // variable declaration only

        private const string VARIABLE_SCOPE_REGEX = @"\b(?:public|private|protected|internal)\b";
        private const string VARIABLE_MODIFIER_REGEX = @"\b(?:static|const)\b";
        private const string VARIABLE_READONLY_REGEX = @"\b(?:readonly)\b";
        private const string VARIABLE_TYPE_REGEX = @"\w+\??(?=\s+\w+)";
        private const string VARIABLE_NAME_REGEX = @"[\w.]+(?=\s*[=;])";
        private const string EQUALITY_REGEX = @"\s*=\s*";
        private const string VARIABLE_VALUE_REGEX = @"\s*\K[\s\S]+(?=;)";

        private static readonly string[] DEF =
        {
            VARIABLE_SCOPE_REGEX,
            VARIABLE_MODIFIER_REGEX,
            VARIABLE_READONLY_REGEX,
            VARIABLE_TYPE_REGEX,
            VARIABLE_NAME_REGEX,
            EQUALITY_REGEX,
            VARIABLE_VALUE_REGEX
        };

        // Inputs
        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(DECLARATION_OR_ASSIGNMENT_FILTER_PORT_NAME, true, typeof(DeclarationOrAssignment), DeclarationOrAssignment.Assignment | DeclarationOrAssignment.Declaration, true)]
        public DeclarationOrAssignment DeclarationOrAssignmentFilter;

        [NodePropertyPort(VARIABLE_TYPE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string VariableTypeFilter;

        [NodePropertyPort(VARIABLE_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string VariableNameFilter;

        [NodePropertyPort(VARIABLE_VALUE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string VariableValueFilter;

        // Outputs
        [NodePropertyPort(VARIABLE_BLOCK_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition VariableBlock;

        [NodePropertyPort(VARIABLE_SCOPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition VariableScope;

        [NodePropertyPort(VARIABLE_MODIFIER_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition VariableModifier;

        [NodePropertyPort(VARIABLE_READONLY_PORT_NAME, false, typeof(bool), false, false)]
        public bool VariableReadonly;

        [NodePropertyPort(VARIABLE_TYPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition VariableType;

        [NodePropertyPort(VARIABLE_NAME_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition VariableName;

        [NodePropertyPort(VARIABLE_VALUE_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition VariableValue;

        [NodePropertyPort(IS_DECLARATION_PORT_NAME, false, typeof(bool), false, false)]
        public bool IsDeclaration;

        [NodePropertyPort(IS_ASSIGNMENT_PORT_NAME, false, typeof(bool), false, false)]
        public bool IsAssignment;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByVariableNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            VariableBlock = null;
            VariableScope = null;
            VariableModifier = null;
            VariableReadonly = false;
            VariableType = null;
            VariableName = null;
            VariableValue = null;
            IsDeclaration = false;
            IsAssignment = false;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            PartitionVariableAssignments(Partition);
        }

        private void PartitionVariableAssignments(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, VARIABLE_REGEX);
            foreach (var p in partitions)
            {
                VariableBlock = p;
                if (ExecutionState == ExecutionState.Failed)
                {
                    return;
                }
                PartitionVariableAssignment(p);
            }
        }

        private void PartitionVariableAssignment(Partition partition)
        {
            var def = Partition.PartitionByRegexMatch(partition, DEF);
            VariableScope = def[0];
            VariableModifier = def[1];
            VariableReadonly = def[2] != null;
            VariableType = def[3];
            IsDeclaration = VariableType != null;
            VariableName = def[4];
            IsAssignment = def[5] != null;
            VariableValue = def[6];

            if (ApplyFilter())
            {
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                }
            }
        }

        private bool ApplyFilter()
        {
            DeclarationOrAssignmentFilter = GetPortValue(DECLARATION_OR_ASSIGNMENT_FILTER_PORT_NAME, DeclarationOrAssignmentFilter);
            if (IsDeclaration && (DeclarationOrAssignmentFilter & DeclarationOrAssignment.Declaration) == 0)
            {
                return false;
            }
            if (IsAssignment && (DeclarationOrAssignmentFilter & DeclarationOrAssignment.Assignment) == 0)
            {
                return false;
            }
            VariableTypeFilter = GetPortValue(VARIABLE_TYPE_FILTER_PORT_NAME, VariableTypeFilter);
            if (!Partition.IsMatch(VariableType, VariableTypeFilter))
            {
                return false;
            }
            VariableNameFilter = GetPortValue(VARIABLE_NAME_FILTER_PORT_NAME, VariableNameFilter);
            if (!Partition.IsMatch(VariableName, VariableNameFilter))
            {
                return false;
            }
            VariableValueFilter = GetPortValue(VARIABLE_VALUE_FILTER_PORT_NAME, VariableValueFilter);
            if (string.IsNullOrEmpty(VariableValueFilter))
            {
                return true;
            }
            if (!IsAssignment || !Partition.IsMatch(VariableValue, VariableValueFilter))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}