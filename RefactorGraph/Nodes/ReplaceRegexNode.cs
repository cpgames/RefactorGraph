using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.ReplaceRegex)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class ReplaceRegexNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string SEARCH_PATTERN_PORT_NAME = "SearchPattern";
        public const string REPLACEMENT_PATTERN_PORT_NAME = "ReplacementPattern";
        public const string CHUNK_PORT_NAME = "Chunk";
        public const string VARIABLES_PORT_NAME = "VariableCollection";
        public const string VARIABLE_1_PORT_NAME = "Variable1";
        public const string VARIABLE_2_PORT_NAME = "Variable2";
        public const string VARIABLE_3_PORT_NAME = "Variable3";

        [NodePropertyPort(CHUNK_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Source\n[Chunk]")]
        public Chunk Chunk;

        [NodePropertyPort(SEARCH_PATTERN_PORT_NAME, true, typeof(string), "Old text", true)]
        public string SearchPattern;

        [NodePropertyPort(REPLACEMENT_PATTERN_PORT_NAME, true, typeof(string), "New text", true)]
        public string ReplacementPattern;

        [NodePropertyPort(VARIABLES_PORT_NAME, true, typeof(ChunkCollection), null, false, DisplayName = "VariableCollection (Optional)\n[ChunkCollection]")]
        public ChunkCollection VariableCollection;
        
        [NodePropertyPort(VARIABLE_1_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Variable 1 (optional)\n[Chunk]")]
        public Chunk Variable1;

        [NodePropertyPort(VARIABLE_2_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Variable 2 (optional)\n[Chunk]")]
        public Chunk Variable2;

        [NodePropertyPort(VARIABLE_3_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Variable 3 (optional)\n[Chunk]")]
        public Chunk Variable3;
        #endregion

        #region Constructors
        public ReplaceRegexNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Chunk = GetPortValue<Chunk>(CHUNK_PORT_NAME);
            SearchPattern = GetPortValue(SEARCH_PATTERN_PORT_NAME, SearchPattern);
            ReplacementPattern = GetPortValue(REPLACEMENT_PATTERN_PORT_NAME, ReplacementPattern);
            VariableCollection = GetPortValue(VARIABLES_PORT_NAME, new ChunkCollection());
            Variable1 = GetPortValue<Chunk>(VARIABLE_1_PORT_NAME);
            Variable2 = GetPortValue<Chunk>(VARIABLE_2_PORT_NAME);
            Variable3 = GetPortValue<Chunk>(VARIABLE_3_PORT_NAME);

            if (Chunk != null)
            {
                ReplaceChunkData();
                _success = true;
            }
        }

        private void ReplaceChunkData()
        {
            var content = Regex.Replace(Chunk.content, SearchPattern, ReplacementPattern, RegexOptions.Multiline);
            var variablesFull = new ChunkCollection();
            variablesFull.Add(Chunk);
            variablesFull.AddRange(VariableCollection);
            if (Variable1 != null)
            {
                variablesFull.Add(Variable1);
            }
            if (Variable2 != null)
            {
                variablesFull.Add(Variable2);
            }
            if (Variable3 != null)
            {
                variablesFull.Add(Variable3);
            }
            foreach (var group in variablesFull.GroupBy(x => x.name))
            {
                var varName = "{" + group.Key + "}";
                var matches = Regex.Matches(content, varName);
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
                    content =
                        content.Substring(0, matches[i].Index) +
                        replacement +
                        content.Substring(matches[i].Index + matches[i].Length);
                }
            }
            Chunk.content = content;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}