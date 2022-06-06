using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.SplitIndex)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class SplitIndexNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string INDEX_PORT_NAME = "Index";
        public const string IGNORE_WHITESPACE_PORT_NAME = "IgnoreWhitespace";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(INDEX_PORT_NAME, true, typeof(int), 0, true)]
        public int Index;

        [NodePropertyPort(IGNORE_WHITESPACE_PORT_NAME, true, typeof(bool), true, true)]
        public bool IgnoreWhitespace;

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, false)]
        public Chunk Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(ChunkCollection), null, true)]
        public ChunkCollection Result;
        #endregion

        #region Constructors
        public SplitIndexNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Index = GetPortValue(INDEX_PORT_NAME, Index);
            var source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            if (source != null)
            {
                var capturedChunks = ProcessChunk(source, Index);
                SetPortValue(RESULT_PORT_NAME, capturedChunks);
                _success = true;
            }
            else
            {
                SetPortValue(RESULT_PORT_NAME, null);
            }
        }

        private ChunkCollection ProcessChunk(Chunk chunk, int index)
        {
            var chunks = new ChunkCollection();
            if (!string.IsNullOrEmpty(chunk.content))
            {
                var content = chunk.content;
                var ignoreWhitespace = GetPortValue(IGNORE_WHITESPACE_PORT_NAME, IgnoreWhitespace);

                if (content.Length <= index)
                {
                    if (content.Length > 0)
                    {
                        if (!ignoreWhitespace || content.Trim().Length > 0)
                        {
                            chunks.Add(chunk);
                        }
                    }
                }
                else
                {
                    var firstPart = content.Substring(0, index);
                    var secondPart = content.Substring(index);
                    if (firstPart.Length > 0)
                    {
                        if (!ignoreWhitespace || firstPart.Trim().Length > 0)
                        {
                            var subChunk = new Chunk
                            {
                                content = firstPart
                            };
                            chunks.Add(subChunk);
                        }
                    }
                    if (secondPart.Length > 0)
                    {
                        if (!ignoreWhitespace || secondPart.Trim().Length > 0)
                        {
                            var subChunk = new Chunk
                            {
                                content = secondPart
                            };
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