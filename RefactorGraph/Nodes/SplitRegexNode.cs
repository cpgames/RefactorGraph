using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.SplitRegex)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class SplitRegexNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string MATCH_NAME_PORT_NAME = "MatchName";
        public const string MISMATCH_NAME_PORT_NAME = "MismatchName";
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string IGNORE_WHITESPACE_PORT_NAME = "IgnoreWhitespace";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(MATCH_NAME_PORT_NAME, true, typeof(string), "MyMatch", true)]
        public string MatchName;

        [NodePropertyPort(MISMATCH_NAME_PORT_NAME, true, typeof(string), "NotMyMatch", true)]
        public string MismatchName;

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(string), "Regex Pattern", true)]
        public string Pattern;

        [NodePropertyPort(IGNORE_WHITESPACE_PORT_NAME, true, typeof(bool), true, true)]
        public bool IgnoreWhitespace;

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, false)]
        public Chunk Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(ChunkCollection), null, true)]
        public ChunkCollection Result;
        #endregion

        #region Constructors
        public SplitRegexNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
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
            MismatchName = GetPortValue(MISMATCH_NAME_PORT_NAME, MismatchName);
            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            Source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            if (Source != null && !string.IsNullOrEmpty(Pattern))
            {
                SplitChunk();
                SetPortValue(RESULT_PORT_NAME, Result);
                _success = true;
            }
            else
            {
                SetPortValue(RESULT_PORT_NAME, null);
            }
        }

        private void SplitChunk()
        {
            Result = new ChunkCollection();
            if (!string.IsNullOrEmpty(Source.content))
            {
                var content = Source.content;
                var ignoreWhitespace = GetPortValue(IGNORE_WHITESPACE_PORT_NAME, IgnoreWhitespace);
                var matches = Regex.Matches(content, Pattern, RegexOptions.Multiline);
                if (matches.Count > 0)
                {
                    for (var i = 0; i < matches.Count; i++)
                    {
                        var index = i == 0 ? 0 : matches[i - 1].Index + matches[i - 1].Length;
                        var length = matches[i].Index - index;
                        if (length > 0)
                        {
                            var subChunk = new Chunk
                            {
                                name = MismatchName,
                                content = content.Substring(index, length),
                                index = index,
                                length = length
                            };
                            if (!ignoreWhitespace || !string.IsNullOrEmpty(subChunk.content.Trim()))
                            {
                                Result.Add(subChunk);
                            }
                        }
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
                {
                    var index = matches.Count > 0 ? matches[matches.Count - 1].Index + matches[matches.Count - 1].Length : 0;
                    var length = content.Length - index;
                    if (length > 0)
                    {
                        var subChunk = new Chunk
                        {
                            name = MismatchName,
                            content = content.Substring(index, length),
                            index = index,
                            length = length - index
                        };
                        if (!ignoreWhitespace || !string.IsNullOrEmpty(subChunk.content.Trim()))
                        {
                            Result.Add(subChunk);
                        }
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