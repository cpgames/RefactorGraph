using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.CreateChunk)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class CreateChunkNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string NAME_PORT_NAME = "Name";
        public const string DATA_PORT_NAME = "Data";
        public const string INDEX_PORT_NAME = "Index";
        public const string RESULT_PORT_NAME = "Result";
        public const string VARIABLE_COLLECTION_PORT_NAME = "VariableCollection";
        public const string VARIABLE_1_PORT_NAME = "Variable1";
        public const string VARIABLE_2_PORT_NAME = "Variable2";
        public const string VARIABLE_3_PORT_NAME = "Variable3";

        [NodePropertyPort(NAME_PORT_NAME, true, typeof(string), "MyChunk", true)]
        public string Name;

        [NodePropertyPort(DATA_PORT_NAME, true, typeof(string), "Chunk Text", true)]
        public string Data;

        [NodePropertyPort(INDEX_PORT_NAME, true, typeof(int), 0, true)]
        public int Index;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(Chunk), null, false)]
        public Chunk Result;

        [NodePropertyPort(VARIABLE_COLLECTION_PORT_NAME, true, typeof(ChunkCollection), null, false, DisplayName = "VariableCollection (Optional)\n[ChunkCollection]")]
        public ChunkCollection VariableCollection;

        [NodePropertyPort(VARIABLE_1_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Variable 1 (optional)\n[Chunk]")]
        public Chunk Variable1;

        [NodePropertyPort(VARIABLE_2_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Variable 2 (optional)\n[Chunk]")]
        public Chunk Variable2;

        [NodePropertyPort(VARIABLE_3_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Variable 3 (optional)\n[Chunk]")]
        public Chunk Variable3;
        #endregion

        #region Constructors
        public CreateChunkNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Name = GetPortValue(NAME_PORT_NAME, Name);
            Data = GetPortValue(DATA_PORT_NAME, Data);
            Index = GetPortValue(INDEX_PORT_NAME, Index);
            VariableCollection = GetPortValue(VARIABLE_COLLECTION_PORT_NAME, new ChunkCollection());
            Variable1 = GetPortValue<Chunk>(VARIABLE_1_PORT_NAME);
            Variable2 = GetPortValue<Chunk>(VARIABLE_2_PORT_NAME);
            Variable3 = GetPortValue<Chunk>(VARIABLE_3_PORT_NAME);

            var chunk = CreateChunk();
            SetPortValue(RESULT_PORT_NAME, chunk);
            _success = true;
        }

        private Chunk CreateChunk()
        {
            var chunk = new Chunk
            {
                name = Name,
                content = Data,
                index = Index
            };
            var variablesFull = new ChunkCollection();
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
                var matches = Regex.Matches(chunk.content, varName);
                var list = group.ToList();
                for (var i = 0; i < matches.Count; i++)
                {
                    string replacement  ;
                    if (i >= list.Count)
                    {
                        replacement = list[list.Count - 1].content;
                    }
                    else
                    {
                        replacement = list[i].content;
                    }
                    chunk.content =
                        chunk.content.Substring(0, matches[i].Index) +
                        replacement +
                        chunk.content.Substring(matches[i].Index + matches[i].Length);
                }
            }
            return chunk;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}