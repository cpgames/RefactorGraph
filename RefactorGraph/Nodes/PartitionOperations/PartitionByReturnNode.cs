using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByReturn)]
    public class PartitionByReturnNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        public const string RETURN_VALUE_FILTER_PORT_NAME = "ReturnValueFilter";

        public const string RETURN_BLOCK_PORT_NAME = "ReturnBlock";
        public const string RETURN_VALUE_PORT_NAME = "ReturnValue";

        private const string RETURN_REGEX = "return\\s*" +
            "(?:[^;(){}<>\"]|" + // non-brackets
            "(<(?:[^<>]++|(?-1))*>)|" + // <> brackets
            "(\\((?:[^()]++|(?-1))*\\))|" + // () brackets
            "(\"(?:[^\"\"]++|(?-1))*\")|" + // quotes
            "({(?:[^{}]++|(?-1))*}))*;"; // {} brackets
        private const string RETURN_VALUE_REGEX = @"return\s*\K[\s\S]*(?=;)";

        // Inputs
        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(RETURN_VALUE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ReturnValueFilter;

        // Outputs
        [NodePropertyPort(RETURN_BLOCK_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition ReturnBlock;

        [NodePropertyPort(RETURN_VALUE_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition ReturnValue;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByReturnNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            ReturnBlock = null;
            ReturnValue = null;
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
            PartitionReturns(Partition);
        }

        private void PartitionReturns(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, RETURN_REGEX);
            var executionState = ExecutionState;
            foreach (var p in partitions)
            {
                ReturnBlock = p;
                if (executionState == ExecutionState.Failed || executionState == ExecutionState.Skipped)
                {
                    return;
                }
                ReturnValue = Partition.PartitionByFirstRegexMatch(ReturnBlock, RETURN_VALUE_REGEX);

                if (ApplyFilter())
                {
                    executionState = ExecutePort(LOOP_PORT_NAME);
                }
            }
            if (executionState == ExecutionState.Failed)
            {
                ExecutionState = ExecutionState.Failed;
            }
        }

        private bool ApplyFilter()
        {
            ReturnValueFilter = GetPortValue(RETURN_VALUE_FILTER_PORT_NAME, ReturnValueFilter);
            if (!Partition.IsMatch(ReturnValue, ReturnValueFilter))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}