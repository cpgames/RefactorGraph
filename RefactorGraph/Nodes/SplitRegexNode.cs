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
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string IGNORE_WHITESPACE_PORT_NAME = "IgnoreWhitespace";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(PATTERN_PORT_NAME, true, typeof(Pattern), null, true, ViewModelType = typeof(PatternPropertyPortViewModel))]
        public Pattern Pattern;

        [NodePropertyPort(IGNORE_WHITESPACE_PORT_NAME, true, typeof(bool), true, true)]
        public bool IgnoreWhitespace;

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, false)]
        public Chunk Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(ChunkCollection), null, true)]
        public ChunkCollection Result;
        #endregion

        #region Constructors
        public SplitRegexNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.SplitRegex)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            SetPortValue(PATTERN_PORT_NAME, new Pattern());
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Pattern = GetPortValue(PATTERN_PORT_NAME, Pattern);
            var source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            if (source != null && !string.IsNullOrEmpty(Pattern.content))
            {
                var capturedChunks = ProcessChunk(source, Pattern);
                SetPortValue(RESULT_PORT_NAME, capturedChunks);
                _success = true;
            }
            else
            {
                SetPortValue(RESULT_PORT_NAME, null);
            }
        }

        private ChunkCollection ProcessChunk(Chunk chunk, Pattern pattern)
        {
            var chunks = new ChunkCollection();
            if (!string.IsNullOrEmpty(chunk.content))
            {
                var content = chunk.content;
                var ignoreWhitespace = GetPortValue(IGNORE_WHITESPACE_PORT_NAME, IgnoreWhitespace);
                var matches = Regex.Matches(content, pattern.content, RegexOptions.Multiline);
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
                                name = $"!{pattern.name}",
                                content = content.Substring(index, length),
                                index = index,
                                length = length
                            };
                            if (!ignoreWhitespace || !string.IsNullOrEmpty(subChunk.content.Trim()))
                            {
                                chunks.Add(subChunk);
                            }
                        }
                        {
                            var subChunk = new Chunk
                            {
                                name = pattern.name,
                                content = matches[i].Value,
                                index = matches[i].Index,
                                length = matches[i].Length
                            };
                            chunks.Add(subChunk);
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
                            name = $"!{pattern.name}",
                            content = content.Substring(index, length),
                            index = index,
                            length = length - index
                        };
                        if (!ignoreWhitespace || !string.IsNullOrEmpty(subChunk.content.Trim()))
                        {
                            chunks.Add(subChunk);
                        }
                    }
                }
            }
            return chunks;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}