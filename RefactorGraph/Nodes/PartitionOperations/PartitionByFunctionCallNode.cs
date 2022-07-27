using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByFunctionCall)]
    public class PartitionByFunctionCallNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        public const string FUNCTION_NAME_FILTER_PORT_NAME = "FunctionNameFilter";
        public const string PARAMETER_FILTER_PORT_NAME = "ParameterFilter";

        public const string FUNCTION_CALL_PORT_NAME = "FunctionCall";
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string PARAMETERS_PORT_NAME = "Parameters";

        private const string FUNCTION_CALL_REGEX = @"(?:new\s*)*" + // new keyword is optional
            @"\w[.\w]*\b" + // function name
            @"(<(?:[^<>]++|(?-1))*>)*\s*" + // generic parameters
            @"(\((?:[^()]++|(?-1))*\))" + // function parameters
            @"(?!\s*[{:\w])"; // exclude non function call statements
        private const string NAME_REGEX = @"[^\(]+[^\s\(](?=\s*\()";
        private const string PARAMS_BLOCK_REGEX = @"\(\s*\K[\s\S]*[^\s](?=\s*\))";
        
        private static readonly string[] NAME_PARAMS = { NAME_REGEX, PARAMS_BLOCK_REGEX };

        // Inputs
        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(FUNCTION_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string FunctionNameFilter;

        [NodePropertyPort(PARAMETER_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ParameterFilter;

        // Outputs
        [NodePropertyPort(FUNCTION_CALL_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition FunctionCall;

        [NodePropertyPort(FUNCTION_NAME_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition FunctionName;

        [NodePropertyPort(PARAMETERS_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition Parameters;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByFunctionCallNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            FunctionCall = null;
            FunctionName = null;
            Parameters = null;
            base.OnPreExecute(prevConnector);
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
            PartitionFunctionCalls(Partition);
        }

        private void PartitionFunctionCalls(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, FUNCTION_CALL_REGEX);
            if (partitions != null)
            {
                foreach (var p in partitions)
                {
                    if (ExecutionState == ExecutionState.Failed)
                    {
                        return;
                    }
                    PartitionFunctionCall(p);
                }
            }
        }

        private void PartitionFunctionCall(Partition partition)
        {
            var name_params = Partition.PartitionByRegexMatch(partition, NAME_PARAMS);
            FunctionCall = partition;
            FunctionName = name_params[0];
            Parameters = name_params[1];

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
                PartitionFunctionCalls(Parameters);
            }
        }

        private bool ApplyFilter()
        {
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