using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.Replace)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class ReplaceNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string PATTERN_PORT_NAME = "Pattern";
        public const string SOURCE_PORT_NAME = "Source";
        public const string VARIABLES_PORT_NAME = "Variables";
        public const string RESULT_PORT_NAME = "Result";
        private Pattern _pattern = new Pattern();
        #endregion

        #region Properties
        public Pattern Pattern
        {
            get => _pattern;
            set => _pattern = value;
        }
        #endregion

        #region Constructors
        public ReplaceNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.Replace)
        {
            Header = "Replace";
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(Pattern), new Pattern(),
                PATTERN_PORT_NAME, true, typeof(PatternPropertyPortViewModel), PATTERN_PORT_NAME);

            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(Chunk), null,
                SOURCE_PORT_NAME, false, null, SOURCE_PORT_NAME);

            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, true, typeof(ChunkCollection), new ChunkCollection(),
                VARIABLES_PORT_NAME, false, null, VARIABLES_PORT_NAME, true);

            NodeGraphManager.CreateNodePropertyPort(false, Guid.NewGuid(), this, false, typeof(Chunk), null,
                RESULT_PORT_NAME, false, null, RESULT_PORT_NAME);
            base.OnCreate();
        }

        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var pattern = GetPortValue(PATTERN_PORT_NAME, _pattern);
            var sourceChunk = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            var variables = GetPortValue(VARIABLES_PORT_NAME, new ChunkCollection());

            if (sourceChunk != null)
            {
                var result = ProcessChunk(sourceChunk, variables, pattern);
                SetPortValue(RESULT_PORT_NAME, result);
                _success = true;
            }
            else
            {
                SetPortValue(RESULT_PORT_NAME, null);
            }
        }

        private Chunk ProcessChunk(Chunk sourceChunk, ChunkCollection variables, Pattern pattern)
        {
            if (sourceChunk.name != pattern.name)
            {
                return sourceChunk;
            }
            var processedChunk = new Chunk
            {
                index = sourceChunk.index,
                content = pattern.content
            };
            var variablesFull = new ChunkCollection();
            variablesFull.Add(sourceChunk);
            variablesFull.AddRange(variables);
            foreach (var group in variablesFull.GroupBy(x => x.name))
            {
                var varName = "{" + group.Key + "}";
                var matches = Regex.Matches(processedChunk.content, varName);
                var list = group.ToList();
                for (var i = 0; i < matches.Count; i++)
                {
                    var replacement = string.Empty;
                    if (i >= list.Count)
                    {
                        replacement = list[list.Count - 1].content;
                    }
                    else
                    {
                        replacement = list[i].content;
                    }
                    processedChunk.content =
                        processedChunk.content.Substring(0, matches[i].Index) +
                        replacement +
                        processedChunk.content.Substring(matches[i].Index + matches[i].Length);
                }
            }

            foreach (var additionalChunk in variables)
            {
                var chunkVar = "{" + additionalChunk.name + "}";
                processedChunk.content = processedChunk.content.Replace(chunkVar, additionalChunk.content);
            }
            return processedChunk;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}