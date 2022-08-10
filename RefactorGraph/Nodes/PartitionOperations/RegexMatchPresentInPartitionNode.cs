using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.RegexMatchPresentInPartition)]
    [NodeFlowPort(TRUE_PORT_NAME, TRUE_PORT_NAME, false)]
    [NodeFlowPort(FALSE_PORT_NAME, FALSE_PORT_NAME, false)]
    public class RegexMatchPresentInPartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string PARTITION_PORT_NAME = "Partition";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;
        #endregion

        #region Properties
        protected override bool HasDone => false;
        #endregion

        #region Constructors
        public RegexMatchPresentInPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (Partition == null || string.IsNullOrEmpty(Pattern))
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            var result = Partition.IsMatch(Partition, Pattern, RegexOptions);
            ExecutionState = ExecutePort(result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
        }
        #endregion
    }
}