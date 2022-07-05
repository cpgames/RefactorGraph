using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByRegexMatch)]
    public class PartitionByRegexMatchNode : RefactorNodeBase
    {
        #region Fields
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string PARTITION_IN_PORT_NAME = "PartitionIn";
        public const string PARTITION_OUT_PORT_NAME = "PartitionOut";

        [NodePropertyPort(PARTITION_IN_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition PartitionIn;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(PARTITION_OUT_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition PartitionOut;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByRegexMatchNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            PartitionOut = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            PartitionIn = GetPortValue<Partition>(PARTITION_IN_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (PartitionIn == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            PartitionByRegexMatches(PartitionIn);
        }

        private void PartitionByRegexMatches(Partition partition)
        {
            var partitions = partition.PartitionByRegexMatch(Pattern);
            foreach (var p in partitions)
            {
                PartitionOut = p;
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }
        }
        #endregion
    }
}