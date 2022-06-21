using System;
using System.Collections.Generic;
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
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string FUNCTION_PARAMETERS_PORT_NAME = "FunctionParameters";

        private const string FUNCTION_CALL_REGEX = @"(?<!\w\s)\b[a-zA-Z][^()\s]*\((?:((?R))|[^()])*\)|(?<=return\s)\b[a-zA-Z][^()\s]*\((?:((?R))|[^()])*\)";
        private const string FUNCTION_NAME_REGEX = @"[^\s]*(?=\()";
        private const string FUNCTION_BODY_REGEX = @"(?<=\().*(?=\))";
        private const string FUNCTION_PARAMS_REGEX = @"[^,\s*][a-zA-Z\s_<>.()0-9]*";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

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
                _success = PartitionFunctionContent(functionCall);
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
                .PartitionByFirstRegexMatch(FUNCTION_BODY_REGEX, PcreOptions.MultiLine);
            if (functionCallBodyContent == null)
            {
                return false;
            }
            FunctionParameters = functionCallBodyContent
                .PartitionByAllRegexMatches(FUNCTION_PARAMS_REGEX, PcreOptions.MultiLine);
            SetPortValue(FUNCTION_PARAMS_REGEX, FunctionParameters);
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