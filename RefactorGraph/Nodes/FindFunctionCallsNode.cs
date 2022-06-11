using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.FindFunctionCalls)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class FindFunctionCallsNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string MATCH_NAME_PORT_NAME = "MatchName";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        private const string FUNCTION_CALL_REGEX = @"(?<!\w\s)\b[a-zA-Z][^()\s]*\((?:((?R))|[^()])*\)|(?<=return\s)\b[a-zA-Z][^()\s]*\((?:((?R))|[^()])*\)";

        [NodePropertyPort(MATCH_NAME_PORT_NAME, true, typeof(string), "MyMatch", true)]
        public string MatchName;

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, false)]
        public Chunk Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(ChunkCollection), null, true)]
        public ChunkCollection Result;
        #endregion

        #region Constructors
        public FindFunctionCallsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            MatchName = GetPortValue(MATCH_NAME_PORT_NAME, MatchName);
            Source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            if (Source != null)
            {
                FindFunctions();
                SetPortValue(RESULT_PORT_NAME, Result);
                _success = true;
            }
            else
            {
                SetPortValue(RESULT_PORT_NAME, null);
            }
        }

        private void FindFunctions()
        {
            Result = new ChunkCollection();
            if (!string.IsNullOrEmpty(Source.content))
            {
                var content = Source.content;
                var matches = Regex.Matches(content, FUNCTION_CALL_REGEX, RegexOptions.Multiline);
                if (matches.Count > 0)
                {
                    for (var i = 0; i < matches.Count; i++)
                    {
                        var subChunk = new Chunk
                        {
                            name = MatchName,
                            content = matches[i].Value,
                            index = matches[i].Index,
                            length = matches[i].Length
                        };
                        Result.Add(subChunk);
                    }
                }
            }
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}