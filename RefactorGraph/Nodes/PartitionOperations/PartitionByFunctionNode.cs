using System;
using System.Collections.Generic;
using System.Linq;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByFunction)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop", false)]
    public class PartitionByFunctionNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string SOURCE_PORT_NAME = "Source";

        public const string SCOPE_FILTER_PORT_NAME = "ScopeFilter";
        public const string MODIFIER_FILTER_PORT_NAME = "ModifierFilter";
        public const string RETURN_TYPE_FILTER_PORT_NAME = "ReturnTypeFilterRegex";
        public const string FUNCTION_NAME_FILTER_PORT_NAME = "FunctionNameFilterRegex";
        public const string PARAMETER_NAME_FILTER_PORT_NAME = "ParameterNameFilterRegex";

        public const string FUNCTION_BLOCK_PORT_NAME = "FunctionBlock";
        public const string FUNCTION_DEF_PORT_NAME = "FunctionDef";
        public const string SCOPE_PORT_NAME = "Scope";
        public const string MODIFIER_PORT_NAME = "Modifier";
        public const string RETURN_TYPE_PORT_NAME = "ReturnType";
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string FUNCTION_PARAMETERS_PORT_NAME = "FunctionParameters";
        public const string FUNCTION_BODY_PORT_NAME = "FunctionBody";

        private const string FUNCTION_BLOCK_REGEX = @"(?:public\s*|private\s*|protected\s*|internal\s*)?" + // scope
            @"(?:abstract\s*|static\s*|override\s*)?" + // modifier
            @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?)\s*" + // return type
            @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?\s*" + // function name
            @"(\((?:[^()]++|(?-1))*\)))" + // function parameters
            @"[\s\w:,]*" + // where clause
            @"({(?:[^{}]++|(?-1))*})"; // function body
        private const string FUNCTION_DEFINITION_REGEX = @"[\s\S]+?(?=\s*{)";
        private const string SCOPE_REGEX = @"(?:public|private|protected|internal)";
        private const string MODIFIER_REGEX = @"(?:abstract|static|override)";
        private const string RETURN_TYPE_REGEX = @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?)";
        private const string FUNCTION_NAME_REGEX = @"(?:\b[\w.]+\b\s*)(<(?:[^<>]++|(?-1))*>)?";
        private const string FUNCTION_PARAMS_BLOCK_REGEX = @"(\((?:[^()]++|(?-1))*\))";
        private const string FUNCTION_PARAMS_REGEX = "(?:\\b[\\w\\s.]+\\b|" + // words
            "(<(?:[^<>]++|(?-1))*>)|" + // <> brackets
            "(\\((?:[^()]++|(?-1))*\\))|" + // () brackets
            "(\"(?:[^\"\"]++|(?-1))*\")|" + // quotes
            "\\s*=>\\s*|" + // lambda
            "({(?:[^{}]++|(?-1))*}))+"; // {} brackets
        private const string FUNCTION_BODY_BLOCK_REGEX = @"({(?:[^{}]++|(?-1))*})";
        private const string FUNCTION_BODY_REGEX = @"(?<={)[\S\s]*(?=\s*})";

        // Inputs
        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Protected | RefactorGraph.Scope.Private | RefactorGraph.Scope.Internal | RefactorGraph.Scope.Public, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(MODIFIER_FILTER_PORT_NAME, true, typeof(FunctionModifier), FunctionModifier.Static | FunctionModifier.Abstract | FunctionModifier.Virtual | FunctionModifier.None, true)]
        public FunctionModifier ModifierFilter;

        [NodePropertyPort(RETURN_TYPE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ReturnTypeFilterRegex;

        [NodePropertyPort(FUNCTION_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string FunctionNameFilterRegex;

        [NodePropertyPort(PARAMETER_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ParameterNameFilterRegex;

        // Outputs
        [NodePropertyPort(FUNCTION_BLOCK_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionBlock;

        [NodePropertyPort(FUNCTION_DEF_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionDef;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Scope;

        [NodePropertyPort(MODIFIER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Modifier;

        [NodePropertyPort(RETURN_TYPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition ReturnType;

        [NodePropertyPort(FUNCTION_NAME_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionName;

        [NodePropertyPort(FUNCTION_PARAMETERS_PORT_NAME, false, typeof(List<Partition>), null, false)]
        public List<Partition> FunctionParameters;

        [NodePropertyPort(FUNCTION_BODY_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionBody;

        private bool _somethingReturned;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        public override bool Success => _somethingReturned;
        #endregion

        #region Constructors
        public PartitionByFunctionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
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
                PartitionFunction(Source);
            }
        }

        private void PartitionFunction(Partition cur)
        {
            var functionBlock = cur.PartitionByFirstRegexMatch(FUNCTION_BLOCK_REGEX, PcreOptions.MultiLine);
            if (functionBlock == null)
            {
                return;
            }
            var functionDef = functionBlock.PartitionByFirstRegexMatch(FUNCTION_DEFINITION_REGEX, PcreOptions.MultiLine);
            var scope = functionDef.PartitionByFirstRegexMatch(SCOPE_REGEX, PcreOptions.MultiLine);
            cur = scope != null ? scope.next : functionDef;
            var modifier = cur.PartitionByFirstRegexMatch(MODIFIER_REGEX, PcreOptions.MultiLine);
            cur = modifier != null ? modifier.next : cur;
            var returnType = cur.PartitionByFirstRegexMatch(RETURN_TYPE_REGEX, PcreOptions.MultiLine);
            cur = returnType != null ? returnType.next : cur;
            var functionName = cur.PartitionByFirstRegexMatch(FUNCTION_NAME_REGEX, PcreOptions.MultiLine);
            cur = functionName.next;
            var functionParamsBlock = cur.PartitionByFirstRegexMatch(FUNCTION_PARAMS_BLOCK_REGEX, PcreOptions.MultiLine);
            var functionParameters = functionParamsBlock.PartitionByAllRegexMatches(FUNCTION_PARAMS_REGEX, PcreOptions.MultiLine);
            cur = functionDef.next;
            var functionBodyBlock = cur.PartitionByFirstRegexMatch(FUNCTION_BODY_BLOCK_REGEX, PcreOptions.MultiLine);
            var functionBody = functionBodyBlock.PartitionByFirstRegexMatch(FUNCTION_BODY_REGEX, PcreOptions.MultiLine);
            if (ApplyFilter(scope, modifier, returnType, functionName, functionParameters))
            {
                FunctionBlock = functionBlock;
                FunctionDef = functionDef;
                Scope = scope;
                Modifier = modifier;
                ReturnType = returnType;
                FunctionName = functionName;
                FunctionParameters = functionParameters;
                FunctionBody = functionBody;
                ExecutePort(LOOP_PORT_NAME);
                _somethingReturned = true;
            }
            cur = functionBodyBlock.next;
            if (cur == null)
            {
                return;
            }
            PartitionFunction(cur);
        }

        private bool ApplyFilter(Partition scope, Partition modifier, Partition returnType, Partition functionName, List<Partition> functionParameters)
        {
            ScopeFilter = GetPortValue(SCOPE_FILTER_PORT_NAME, ScopeFilter);
            var scopeEnum = RefactorGraph.Scope.Scopeless;
            if (scope != null)
            {
                switch (scope.Data)
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
            if (modifier != null)
            {
                switch (modifier.Data)
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
            ReturnTypeFilterRegex = GetPortValue(RETURN_TYPE_FILTER_PORT_NAME, ReturnTypeFilterRegex);
            if (!string.IsNullOrEmpty(ReturnTypeFilterRegex))
            {
                if (!Partition.IsValid(returnType) ||
                    !PcreRegex.IsMatch(returnType.Data, ReturnTypeFilterRegex))
                {
                    return false;
                }
            }

            FunctionNameFilterRegex = GetPortValue(FUNCTION_NAME_FILTER_PORT_NAME, FunctionNameFilterRegex);
            if (!string.IsNullOrEmpty(FunctionNameFilterRegex))
            {
                if (!PcreRegex.IsMatch(functionName.Data, FunctionNameFilterRegex))
                {
                    return false;
                }
            }

            ParameterNameFilterRegex = GetPortValue(PARAMETER_NAME_FILTER_PORT_NAME, ParameterNameFilterRegex);
            if (!string.IsNullOrEmpty(ParameterNameFilterRegex))
            {
                if (functionParameters.Count == 0)
                {
                    return false;
                }
                if (functionParameters.All(x => !PcreRegex.IsMatch(x.Data, ParameterNameFilterRegex)))
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