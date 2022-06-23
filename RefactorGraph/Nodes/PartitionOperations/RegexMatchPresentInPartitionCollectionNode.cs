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
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string SOURCE_PORT_NAME = "Source";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(List<Partition>), null, false)]
        public List<Partition> Source;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public RegexMatchPresentInPartitionCollectionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            Source = GetPortValue<List<Partition>>(SOURCE_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (Source != null && !string.IsNullOrEmpty(Pattern))
            {
                var result = Source.Any(x => PcreRegex.IsMatch(x.Data, Pattern, RegexOptions));
                ExecutePort(result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
            }
        }
        #endregion
    }
}