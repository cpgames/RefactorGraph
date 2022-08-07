using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByFirstRegexMatch)]
    [NodeFlowPort(FOUND_PORT_NAME, "Found", false)]
    [NodeFlowPort(NOT_FOUND_PORT_NAME, "Not Found", false)]
    public class PartitionByFirstRegexMatchNode : RefactorNodeBase
    {
        #region Fields
        public const string FOUND_PORT_NAME = "Found";
        public const string NOT_FOUND_PORT_NAME = "NotFound";
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

        [NodePropertyPort(MATCH_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition Match;
        #endregion

        #region Properties
        protected override bool HasDone => false;
        #endregion

        #region Constructors
        public PartitionByFirstRegexMatchNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
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
            Match = Partition.PartitionByFirstRegexMatch(Partition, Pattern, RegexOptions);
            ExecutePort(Match == null ? NOT_FOUND_PORT_NAME : FOUND_PORT_NAME);
        }
        #endregion
    }
}