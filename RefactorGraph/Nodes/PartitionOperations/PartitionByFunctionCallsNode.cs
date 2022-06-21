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

        private const string FUNCTION_CALL_REGEX = "(?<![\\[\\/])(?:new\\s*)*\\b\\w[\\w.=<>]+\\s*\\((?:((?R))|[^()])*\\)(?!\\s*[{:\"])";
        private const string FUNCTION_NAME_REGEX = @"[^\s]*(?=\()";
        private const string FUNCTION_PARAMS_BLOCK_REGEX = @"(?<=\().*(?=\))";
        private const string FUNCTION_PARAMS_REGEX = @"[^,\s*][\w\s_<>.()]*";

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
        #endregion

        #region Properties
        protected override bool HasOutput => false;
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
                PartitionFunction();
                _success = true;
            }
        }

        private void PartitionFunction()
        {
            var functionCalls = Source.PartitionByAllRegexMatches(FUNCTION_CALL_REGEX, PcreOptions.MultiLine);
            foreach (var functionCall in functionCalls)
            {
                SetPortValue(FUNCTION_CALL_PORT_NAME, functionCall);
                _success = PartitionFunctionContent(functionCall) && ApplyFilter();
                ExecutePort(LOOP_PORT_NAME);
            }
        }

        private bool PartitionFunctionContent(Partition functionCall)
        {
            FunctionName = functionCall
                .PartitionByFirstRegexMatch(FUNCTION_NAME_REGEX, PcreOptions.MultiLine);
            SetPortValue(FUNCTION_NAME_PORT_NAME, FunctionName);
            var functionCallBody = FunctionName?.next;
            var functionCallBodyContent = functionCallBody?
                .PartitionByIndexAndLength(1, functionCallBody.Data.Length - 2);
            if (functionCallBodyContent == null)
            {
                return false;
            }
            FunctionParameters = functionCallBodyContent
                .PartitionByAllRegexMatches(FUNCTION_PARAMS_REGEX, PcreOptions.MultiLine);
            SetPortValue(FUNCTION_PARAMETERS_PORT_NAME, FunctionParameters);
            return true;
        }

        private bool ApplyFilter()
        {
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