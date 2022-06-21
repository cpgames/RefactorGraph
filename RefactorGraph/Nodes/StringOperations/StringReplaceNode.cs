﻿using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.StringOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.StringOperations, RefactorNodeType.StringReplace)]
    public class StringReplaceNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string REGEX_OPTIONS_PORT_NAME = "RegexOptions";
        public const string REPLACEMENT_PORT_NAME = "Replacement";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(string), "", true)]
        public string Source;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "pattern", true)]
        public string Pattern;

        [NodePropertyPort(REPLACEMENT_PORT_NAME, true, typeof(string), "replacement", true)]
        public string Replacement;

        [NodePropertyPort(REGEX_OPTIONS_PORT_NAME, true, typeof(PcreOptions), PcreOptions.MultiLine, true)]
        public PcreOptions RegexOptions;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(string), "", false)]
        public string Result;
        #endregion

        #region Constructors
        public StringReplaceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue(SOURCE_PORT_NAME, Source);
            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            Replacement = GetPortValue(REPLACEMENT_PORT_NAME, Replacement);
            RegexOptions = GetPortValue(REGEX_OPTIONS_PORT_NAME, RegexOptions);
            Result = PcreRegex.Replace(Source, Pattern, Replacement, RegexOptions);
            SetPortValue(RESULT_PORT_NAME, Result);
            _success = true;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}