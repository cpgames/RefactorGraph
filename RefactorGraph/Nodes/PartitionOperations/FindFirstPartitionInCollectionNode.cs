using System;
using System.Collections.Generic;
using System.Linq;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.FindFirstPartitionInCollection)]
    public class FindFirstPartitionInCollectionNode : RefactorNodeBase
    {
        #region Fields
        public const string NOT_FOUND_PORT_NAME = "NotFound";
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string PARTITION_COLLECTION_PORT_NAME = "PartitionCollection";
        public const string PARTITION_PORT_NAME = "Partition";

        [NodePropertyPort(PARTITION_COLLECTION_PORT_NAME, true, typeof(List<Partition>), null, false, Serialized = false)]
        public List<Partition> PartitionCollection;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(PARTITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;
        #endregion

        #region Constructors
        public FindFirstPartitionInCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            CreateOutputFlowPort(NOT_FOUND_PORT_NAME);
        }

        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Partition = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            PartitionCollection = GetPortValue<List<Partition>>(PARTITION_COLLECTION_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (PartitionCollection == null || string.IsNullOrEmpty(Pattern))
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            Partition = PartitionCollection.FirstOrDefault(x => PcreRegex.IsMatch(x.data, Pattern, RegexOptions));
            if (Partition == null)
            {
                ExecutePort(NOT_FOUND_PORT_NAME);
            }
            else
            {
                ExecutionState = ExecutionState.Skipped;
            }
        }
        #endregion
    }
}