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
        public const string PARTITION_PORT_NAME = "Partition";
        public const string MATCH_PORT_NAME = "Match";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(MATCH_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Match;
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
            Match = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            PartitionByRegexMatches(Partition);
        }

        private void PartitionByRegexMatches(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, Pattern);
            foreach (var p in partitions)
            {
                Match = p;
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
                if (executionState == ExecutionState.Skipped)
                {
                    break;
                }
            }
        }
        #endregion
    }
}