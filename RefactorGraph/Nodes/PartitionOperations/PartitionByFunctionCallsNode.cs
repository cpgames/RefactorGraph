using System;
using System.Collections.Generic;
using System.Linq;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByFunctionCall)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop", false)]
    public class PartitionByFunctionCallsNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string SOURCE_PORT_NAME = "Source";
        public const string FUNCTION_NAME_FILTER_PORT_NAME = "FunctionNameFilterRegex";
        public const string PARAMETER_NAME_FILTER_PORT_NAME = "ParameterNameFilterRegex";
        public const string FUNCTION_CALL_PORT_NAME = "FunctionCall";
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string FUNCTION_PARAMETERS_PORT_NAME = "FunctionParameters";

        private const string FUNCTION_CALL_REGEX = @"(?:new\s*)*" + // new keyword is optional
            @"\w[.\w]*\b" + // function name
            @"(<(?:[^<>]++|(?-1))*>)*\s*" + // generic parameters
            @"(\((?:[^()]++|(?-1))*\))" + // function parameters
            @"(?!\s*[{:\w])"; // exclude non function call statements
        private const string FUNCTION_NAME_REGEX = @"[^\s(]+";
        private const string FUNCTION_PARAMS_BLOCK_REGEX = @"\(\s*\K[\s\S]*[^\s](?=\s*\))";
        private const string FUNCTION_PARAMS_REGEX = "(?:\\b[\\w\\s.]+\\b|" + // words
            "(<(?:[^<>]++|(?-1))*>)|" + // <> brackets
            "(\\((?:[^()]++|(?-1))*\\))|" + // () brackets
            "(\"(?:[^\"\"]++|(?-1))*\")|" + // quotes
            "\\s*=>\\s*|" + // lambda
            "({(?:[^{}]++|(?-1))*}))+"; // {} brackets

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(FUNCTION_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string FunctionNameFilterRegex;

        [NodePropertyPort(PARAMETER_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ParameterNameFilterRegex;

        [NodePropertyPort(FUNCTION_CALL_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionCall;

        [NodePropertyPort(FUNCTION_NAME_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition FunctionName;

        [NodePropertyPort(FUNCTION_PARAMETERS_PORT_NAME, false, typeof(List<Partition>), null, true)]
        public List<Partition> FunctionParameters;

        private bool _somethingReturned;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        public override bool Success => _somethingReturned;
        #endregion

        #region Constructors
        public PartitionByFunctionCallsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null && !Source.IsPartitioned)
            {
                PartitionFunctionCall(Source);
            }
        }

        private bool PartitionFunctionCall(Partition cur)
        {
            var functionCall = cur.PartitionByFirstRegexMatch(FUNCTION_CALL_REGEX, PcreOptions.MultiLine);
            if (!Partition.IsValid(functionCall))
            {
                return false;
            }
            var functionName = functionCall.PartitionByFirstRegexMatch(FUNCTION_NAME_REGEX, PcreOptions.MultiLine);
            var paramsBlock = functionName.next;
            var paramsContent = paramsBlock.PartitionByFirstRegexMatch(FUNCTION_PARAMS_BLOCK_REGEX, PcreOptions.MultiLine);
            List<Partition> functionParameters = null;
            if (Partition.IsValid(paramsContent))
            {
                functionParameters = paramsContent.PartitionByAllRegexMatches(FUNCTION_PARAMS_REGEX, PcreOptions.MultiLine);
                foreach (var functionParameter in functionParameters)
                {
                    _somethingReturned |= PartitionFunctionCall(functionParameter);
                }
            }
            if (ApplyFilter(functionName, functionParameters))
            {
                FunctionCall = functionCall;
                FunctionName = functionName;
                FunctionParameters = functionParameters;
                ExecutePort(LOOP_PORT_NAME);
                _somethingReturned = true;
            }
            cur = functionCall.next;
            if (cur != null)
            {
                _somethingReturned |= PartitionFunctionCall(cur);
            }
            return _somethingReturned;
        }

        private bool ApplyFilter(Partition functionName, List<Partition> functionParameters)
        {
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
                if (functionParameters == null || functionParameters.Count == 0)
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