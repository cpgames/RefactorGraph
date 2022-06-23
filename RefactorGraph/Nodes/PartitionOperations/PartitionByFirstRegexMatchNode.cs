using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByFirstRegexMatch)]
    public class PartitionByFirstRegexMatchNode : RefactorNodeBase
    {
        #region Fields
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(Partition), null, true, DisplayName = "Partition")]
        public Partition Result;
        #endregion

        #region Properties
        public override bool Success => Result != null;
        #endregion

        #region Constructors
        public PartitionByFirstRegexMatchNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Result = null;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (Source != null && !Source.IsPartitioned && !string.IsNullOrEmpty(Pattern))
            {
                Result = Source.PartitionByFirstRegexMatch(Pattern, RegexOptions);
                SetPortValue(RESULT_PORT_NAME, Result);
            }
        }
        #endregion
    }
}