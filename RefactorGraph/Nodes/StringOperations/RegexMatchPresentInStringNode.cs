using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.StringOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.StringOperations, RefactorNodeType.RegexMatchPresentInString)]
    [NodeFlowPort(TRUE_PORT_NAME, TRUE_PORT_NAME, false)]
    [NodeFlowPort(FALSE_PORT_NAME, FALSE_PORT_NAME, false)]
    public class RegexMatchPresentInStringNode : RefactorNodeBase
    {
        #region Fields
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string SOURCE_PORT_NAME = "Source";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(string), "", false)]
        public string Source;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex", true)]
        public string Pattern;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, false, typeof(bool), false, false)]
        public bool Result;
        #endregion

        #region Constructors
        public RegexMatchPresentInStringNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue(SOURCE_PORT_NAME, Source);
            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            if (!string.IsNullOrEmpty(Pattern))
            {
                Result = PcreRegex.IsMatch(Source, Pattern, RegexOptions);
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