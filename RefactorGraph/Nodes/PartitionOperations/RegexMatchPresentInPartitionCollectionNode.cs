using System;
using System.Collections.Generic;
using System.Linq;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.RegexMatchPresentInPartitionCollection)]
    [NodeFlowPort(TRUE_PORT_NAME, TRUE_PORT_NAME, false)]
    [NodeFlowPort(FALSE_PORT_NAME, FALSE_PORT_NAME, false)]
    public class RegexMatchPresentInPartitionCollectionNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_COLLECTION_PORT_NAME = "PartitionCollection";
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";
        
        [NodePropertyPort(PARTITION_COLLECTION_PORT_NAME, true, typeof(List<Partition>), null, false, Serialized = false)]
        public List<Partition> PartitionCollection;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;
        #endregion

        #region Properties
        protected override bool HasDone => false;
        #endregion

        #region Constructors
        public RegexMatchPresentInPartitionCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            PartitionCollection = GetPortValue<List<Partition>>(PARTITION_COLLECTION_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (PartitionCollection != null && !string.IsNullOrEmpty(Pattern))
            {
                var result = PartitionCollection.Any(x => PcreRegex.IsMatch(x.data, Pattern, RegexOptions));
                ExecutePort(result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
            }
        }
        #endregion
    }
}