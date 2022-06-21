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
        public const string QUALIFIER_FILTER_PORT_NAME = "QualifierFilter";
        public const string RETURN_TYPE_FILTER_PORT_NAME = "ReturnTypeFilterRegex";
        public const string IS_CONSTRUCTOR_FILTER_PORT_NAME = "IsConstructorFilter";
        public const string FUNCTION_NAME_FILTER_PORT_NAME = "FunctionNameFilterRegex";
        public const string PARAMETER_NAME_FILTER_PORT_NAME = "ParameterNameFilterRegex";

        public const string SCOPE_PORT_NAME = "Scope";
        public const string QUALIFIER_PORT_NAME = "Qualifier";
        public const string RETURN_TYPE_PORT_NAME = "ReturnType";
        public const string IS_CONSTRUCTOR_PORT_NAME = "IsConstructor";
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string FUNCTION_PARAMETERS_PORT_NAME = "FunctionParameters";
        public const string FUNCTION_BODY_PORT_NAME = "FunctionBody";

        private const string FUNCTION_DEFINITION_REGEX = @"(?:public\s*|private\s*|protected\s*|internal\s*)?(?:abstract\s*|static\s*|override\s*)?\w+\s+[\w<>]+?(?=\([^()]*\)\s*[{:])";
        private const string SCOPE_REGEX = @"(?:public\s*|private\s*|protected\s*|internal\s*)";
        private const string QUALIFIER_REGEX = @"(?:abstract\s*|static\s*|override\s*)";
        private const string RETURN_TYPE_REGEX = @"\w+(?=\s+\w+)";
        private const string FUNCTION_NAME_WITH_GENERICS_REGEX = @"\b[\w\n\s,<>]*[\w>](?=\s*:*)*";
        private const string FUNCTION_NAME_REGEX = @"\w+";
        private const string FUNCTION_PARAMS_BLOCK_REGEX = @"\((?:[^()]|(?R))*\)";
        private const string FUNCTION_PARAMS_INNER_REGEX = @"\b(?:\s|.)+(?<!\s)(?=\s*\))";
        private const string FUNCTION_PARAMS_REGEX = "[^,\\s*][\\w\\s<>.()\\[\\]]*[\\w\"\\)]";
        private const string FUNCTION_BODY_BLOCK_REGEX = @"{(?:[^{}]|(?R))*}";
        private const string FUNCTION_BODY_REGEX = @"(?<={)[\S\s]*(?=\s*})";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Any, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(QUALIFIER_FILTER_PORT_NAME, true, typeof(Qualifier), RefactorGraph.Qualifier.Any, true)]
        public Qualifier QualifierFilter;

        [NodePropertyPort(RETURN_TYPE_FILTER_PORT_NAME, true, typeof(string), FUNCTION_NAME_REGEX, true)]
        public string ReturnTypeFilterRegex;

        [NodePropertyPort(IS_CONSTRUCTOR_FILTER_PORT_NAME, true, typeof(bool), false, true)]
        public bool IsConstructorFilter;

        [NodePropertyPort(FUNCTION_NAME_FILTER_PORT_NAME, true, typeof(string), FUNCTION_NAME_REGEX, true)]
        public string FunctionNameFilterRegex;

        [NodePropertyPort(PARAMETER_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ParameterNameFilterRegex;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Scope;

        [NodePropertyPort(QUALIFIER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Qualifier;

        [NodePropertyPort(RETURN_TYPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition ReturnType;

        [NodePropertyPort(IS_CONSTRUCTOR_PORT_NAME, false, typeof(bool), false, false)]
        public bool IsConstructor;

        [NodePropertyPort(FUNCTION_NAME_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionName;

        [NodePropertyPort(FUNCTION_PARAMETERS_PORT_NAME, false, typeof(List<Partition>), null, false)]
        public List<Partition> FunctionParameters;

        [NodePropertyPort(FUNCTION_BODY_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionBody;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public PartitionByFunctionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null && !Source.IsPartitioned)
            {
                _success = PartitionFunctions(Source);
            }
        }

        private bool PartitionFunctions(Partition cur)
        {
            var classDefinition = cur.PartitionByFirstRegexMatch(FUNCTION_DEFINITION_REGEX, PcreOptions.MultiLine);
            if (classDefinition == null)
            {
                return true;
            }
            if (!PartitionFunctionDefinition(classDefinition))
            {
                return false;
            }
            cur = classDefinition.next;
            if (!PartitionFunctionParameters(cur, out cur))
            {
                return false;
            }
            if (cur == null)
            {
                return false;
            }
            var functionBodyBlock = cur.PartitionByFirstRegexMatch(FUNCTION_BODY_BLOCK_REGEX, PcreOptions.MultiLine);
            if (functionBodyBlock == null)
            {
                return false;
            }
            FunctionBody = functionBodyBlock.PartitionByFirstRegexMatch(FUNCTION_BODY_REGEX, PcreOptions.MultiLine);
            SetPortValue(FUNCTION_BODY_PORT_NAME, FunctionBody);
            if (FunctionBody == null)
            {
                return false;
            }
            _success = ApplyFilter();
            ExecutePort(LOOP_PORT_NAME);
            cur = functionBodyBlock.next;
            if (cur == null)
            {
                return true;
            }
            return PartitionFunctions(cur);
        }

        private bool PartitionFunctionDefinition(Partition classDefinition)
        {
            var cur = classDefinition;
            Scope = cur.PartitionByFirstRegexMatch(SCOPE_REGEX, PcreOptions.MultiLine);
            SetPortValue(SCOPE_PORT_NAME, Scope);
            if (Scope != null)
            {
                cur = Scope.next;
            }
            if (cur == null)
            {
                return false;
            }
            Qualifier = cur.PartitionByFirstRegexMatch(QUALIFIER_REGEX, PcreOptions.MultiLine);
            SetPortValue(QUALIFIER_PORT_NAME, Qualifier);
            if (Qualifier != null)
            {
                cur = Qualifier.next;
            }
            if (cur == null)
            {
                return false;
            }
            ReturnType = cur.PartitionByFirstRegexMatch(RETURN_TYPE_REGEX, PcreOptions.MultiLine);
            SetPortValue(RETURN_TYPE_PORT_NAME, ReturnType);
            if (ReturnType != null)
            {
                cur = ReturnType.next;
            }
            IsConstructor = ReturnType == null;
            SetPortValue(IS_CONSTRUCTOR_PORT_NAME, IsConstructor);
            if (cur == null)
            {
                return false;
            }
            var functionNameWithRegex = cur.PartitionByFirstRegexMatch(FUNCTION_NAME_WITH_GENERICS_REGEX, PcreOptions.MultiLine);
            if (functionNameWithRegex == null)
            {
                return false;
            }
            cur = functionNameWithRegex;
            FunctionName = cur.PartitionByFirstRegexMatch(FUNCTION_NAME_REGEX, PcreOptions.MultiLine);
            SetPortValue(FUNCTION_NAME_PORT_NAME, FunctionName);
            return true;
        }

        private bool PartitionFunctionParameters(Partition partition, out Partition next)
        {
            FunctionParameters = new List<Partition>();
            var functionParamsBlock = partition.PartitionByFirstRegexMatch(FUNCTION_PARAMS_BLOCK_REGEX, PcreOptions.MultiLine);
            if (functionParamsBlock == null)
            {
                next = null;
                return false;
            }
            next = functionParamsBlock.next;
            var functionParamsInner = functionParamsBlock.PartitionByFirstRegexMatch(FUNCTION_PARAMS_INNER_REGEX, PcreOptions.MultiLine);
            if (functionParamsInner != null)
            {
                FunctionParameters = functionParamsInner.PartitionByAllRegexMatches(FUNCTION_PARAMS_REGEX, PcreOptions.MultiLine);
            }
            SetPortValue(FUNCTION_PARAMETERS_PORT_NAME, FunctionParameters);
            return true;
        }

        private bool ApplyFilter()
        {
            ScopeFilter = GetPortValue(SCOPE_FILTER_PORT_NAME, ScopeFilter);
            switch (ScopeFilter)
            {
                case RefactorGraph.Scope.None:
                    if (Scope != null)
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Public:
                    if (Scope == null || Scope.Data != "public")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Private:
                    if (Scope == null || Scope.Data != "private")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Internal:
                    if (Scope == null || Scope.Data != "internal")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Protected:
                    if (Scope == null || Scope.Data != "protected")
                    {
                        return false;
                    }
                    break;
            }

            QualifierFilter = GetPortValue(QUALIFIER_FILTER_PORT_NAME, QualifierFilter);
            switch (QualifierFilter)
            {
                case RefactorGraph.Qualifier.None:
                    if (Qualifier != null)
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Qualifier.Static:
                    if (Qualifier == null || Qualifier.Data != "static")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Qualifier.Abstract:
                    if (Qualifier == null || Qualifier.Data != "abstract")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Qualifier.Virtual:
                    if (Qualifier == null || Qualifier.Data != "virtual")
                    {
                        return false;
                    }
                    break;
            }

            if (ReturnType != null)
            {
                ReturnTypeFilterRegex = GetPortValue(RETURN_TYPE_FILTER_PORT_NAME, ReturnTypeFilterRegex);
                if (!string.IsNullOrEmpty(ReturnTypeFilterRegex))
                {
                    if (!PcreRegex.IsMatch(ReturnType.Data, ReturnTypeFilterRegex))
                    {
                        return false;
                    }
                }
            }

            FunctionNameFilterRegex = GetPortValue(FUNCTION_NAME_FILTER_PORT_NAME, FunctionNameFilterRegex);
            if (!string.IsNullOrEmpty(FunctionNameFilterRegex))
            {
                if (!PcreRegex.IsMatch(FunctionName.Data, FunctionNameFilterRegex))
                {
                    return false;
                }
            }

            ParameterNameFilterRegex = GetPortValue(PARAMETER_NAME_FILTER_PORT_NAME, ParameterNameFilterRegex);
            if (!string.IsNullOrEmpty(ParameterNameFilterRegex))
            {
                if (FunctionParameters.Count == 0)
                {
                    return false;
                }
                if (FunctionParameters.All(x => !PcreRegex.IsMatch(x.Data, ParameterNameFilterRegex)))
                {
                    return false;
                }
            }

            IsConstructorFilter = GetPortValue(IS_CONSTRUCTOR_FILTER_PORT_NAME, IsConstructorFilter);
            return IsConstructorFilter == IsConstructor;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(COMPLETED_PORT_NAME);
        }
        #endregion
    }
}