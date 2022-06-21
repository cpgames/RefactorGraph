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
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(List<Partition>), null, false)]
        public List<Partition> Source;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(bool), false, false)]
        public bool Result;
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
                Result = Source.Any(x => PcreRegex.IsMatch(x.Data, Pattern, RegexOptions));
                SetPortValue(RESULT_PORT_NAME, Result);
                _success = true;
            }
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(Result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
        }
        #endregion
    }
}