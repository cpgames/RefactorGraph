using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByFunction)]
    public class PartitionByFunctionNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        public const string SCOPE_FILTER_PORT_NAME = "ScopeFilter";
        public const string MODIFIER_FILTER_PORT_NAME = "ModifierFilter";
        public const string RETURN_TYPE_FILTER_PORT_NAME = "ReturnTypeFilter";
        public const string FUNCTION_NAME_FILTER_PORT_NAME = "FunctionNameFilter";
        public const string PARAMETER_FILTER_PORT_NAME = "ParameterFilter";

        public const string FUNCTION_PORT_NAME = "Function";
        public const string SCOPE_PORT_NAME = "Scope";
        public const string MODIFIER_PORT_NAME = "Modifier";
        public const string RETURN_TYPE_PORT_NAME = "ReturnType";
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string PARAMETERS_PORT_NAME = "Parameters";
        public const string FUNCTION_BODY_PORT_NAME = "FunctionBody";

        private const string FUNCTION_REGEX = @"(?:public\s*|private\s*|protected\s*|internal\s*)?" + // scope
            @"(?:abstract\s*|static\s*|override\s*)?" + // modifier
            @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?)\s*" + // return type
            @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?\s*" + // function name
            @"(\((?:[^()]++|(?-1))*\)))" + // function parameters
            @"[\s\w:,]*" + // where clause
            @"({(?:[^{}]++|(?-1))*})"; // function body
        private const string DEF_REGEX = @"[\s\S]+?(?=\s*{)";
        private const string SCOPE_REGEX = @"(?:public|private|protected|internal)";
        private const string MODIFIER_REGEX = @"(?:abstract|static|override)";
        private const string RETURN_TYPE_REGEX = @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?)";
        private const string NAME_REGEX = @"(?:\b[\w.]+\b\s*)(<(?:[^<>]++|(?-1))*>)?";
        private const string PARAMS_BLOCK_REGEX = @"(\((?:[^()]++|(?-1))*\))";
        private const string PARAMS_REGEX = "(?:\\b[\\w\\s.]+\\b|" + // words
            "(<(?:[^<>]++|(?-1))*>)|" + // <> brackets
            "(\\((?:[^()]++|(?-1))*\\))|" + // () brackets
            "(\"(?:[^\"\"]++|(?-1))*\")|" + // quotes
            "\\s*=>\\s*|" + // lambda
            "({(?:[^{}]++|(?-1))*}))+"; // {} brackets
        private const string BODY_BLOCK_REGEX = @"({(?:[^{}]++|(?-1))*})";
        private const string BODY_REGEX = @"(?<={)[\S\s]*(?=\s*})";

        private static readonly string[] DEF_BODY = { DEF_REGEX, BODY_BLOCK_REGEX };
        private static readonly string[] SCOPE_MODIFIER_RETURN_TYPE_NAME_PARAMS = { SCOPE_REGEX, MODIFIER_REGEX, RETURN_TYPE_REGEX, NAME_REGEX, PARAMS_BLOCK_REGEX };

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Protected | RefactorGraph.Scope.Private | RefactorGraph.Scope.Internal | RefactorGraph.Scope.Public, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(MODIFIER_FILTER_PORT_NAME, true, typeof(FunctionModifier), FunctionModifier.Static | FunctionModifier.Abstract | FunctionModifier.Virtual | FunctionModifier.None, true)]
        public FunctionModifier ModifierFilter;

        [NodePropertyPort(RETURN_TYPE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ReturnTypeFilter;

        [NodePropertyPort(FUNCTION_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string FunctionNameFilter;

        [NodePropertyPort(PARAMETER_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ParameterFilter;
        [NodePropertyPort(FUNCTION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Function;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Scope;

        [NodePropertyPort(MODIFIER_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Modifier;

        [NodePropertyPort(RETURN_TYPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition ReturnType;

        [NodePropertyPort(FUNCTION_NAME_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition FunctionName;

        [NodePropertyPort(PARAMETERS_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition Parameters;

        [NodePropertyPort(FUNCTION_BODY_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition FunctionBody;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByFunctionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Function = null;
            Scope = null;
            Modifier = null;
            ReturnType = null;
            FunctionName = null;
            Parameters = null;
            FunctionBody = null;
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
            PartitionFunctions(Partition);
        }

        private void PartitionFunctions(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, FUNCTION_REGEX);
            foreach (var p in partitions)
            {
                if (ExecutionState == ExecutionState.Failed)
                {
                    return;
                }
                PartitionFunction(p);
            }
        }

        private void PartitionFunction(Partition partition)
        {
            var def_body = Partition.PartitionByRegexMatch(partition, DEF_BODY);
            var scope_modifier_returnType_name_params = Partition.PartitionByRegexMatch(def_body[0], SCOPE_MODIFIER_RETURN_TYPE_NAME_PARAMS);
            Scope = scope_modifier_returnType_name_params[0];
            Modifier = scope_modifier_returnType_name_params[1];
            ReturnType = scope_modifier_returnType_name_params[2];
            FunctionName = scope_modifier_returnType_name_params[3];
            Parameters = Partition.PartitionByFirstRegexMatch(scope_modifier_returnType_name_params[4], PARAMS_REGEX);
            FunctionBody = Partition.PartitionByFirstRegexMatch(def_body[1], BODY_REGEX);
            if (ApplyFilter())
            {
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }
            if (ExecutionState != ExecutionState.Skipped)
            {
                PartitionFunctions(FunctionBody);
            }
        }

        private bool ApplyFilter()
        {
            ScopeFilter = GetPortValue(SCOPE_FILTER_PORT_NAME, ScopeFilter);
            var scopeEnum = RefactorGraph.Scope.Scopeless;
            if (Scope != null)
            {
                switch (Scope.data)
                {
                    case "public":
                        scopeEnum = RefactorGraph.Scope.Public;
                        break;
                    case "private":
                        scopeEnum = RefactorGraph.Scope.Private;
                        break;
                    case "internal":
                        scopeEnum = RefactorGraph.Scope.Internal;
                        break;
                    case "protected":
                        scopeEnum = RefactorGraph.Scope.Protected;
                        break;
                }
            }
            if ((scopeEnum & ScopeFilter) == 0)
            {
                return false;
            }

            ModifierFilter = GetPortValue(MODIFIER_FILTER_PORT_NAME, ModifierFilter);
            var modifierEnum = FunctionModifier.None;
            if (Modifier != null)
            {
                switch (Modifier.data)
                {
                    case "static":
                        modifierEnum = FunctionModifier.Static;
                        break;
                    case "virtual":
                        modifierEnum = FunctionModifier.Virtual;
                        break;
                    case "abstract":
                        modifierEnum = FunctionModifier.Abstract;
                        break;
                }
            }
            if ((modifierEnum & ModifierFilter) == 0)
            {
                return false;
            }

            ReturnTypeFilter = GetPortValue(RETURN_TYPE_FILTER_PORT_NAME, ReturnTypeFilter);
            if (!Partition.IsMatch(ReturnType, ReturnTypeFilter))
            {
                return false;
            }

            FunctionNameFilter = GetPortValue(FUNCTION_NAME_FILTER_PORT_NAME, FunctionNameFilter);
            if (!Partition.IsMatch(FunctionName, FunctionNameFilter))
            {
                return false;
            }

            ParameterFilter = GetPortValue(PARAMETER_FILTER_PORT_NAME, ParameterFilter);
            if (!Partition.IsMatch(Parameters, ParameterFilter))
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}