using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.IntToChunk)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class IntToChunkNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string INT_PORT_NAME = "Int";
        public const string NAME_PORT_NAME = "ChunkName";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(INT_PORT_NAME, true, typeof(int), 0, true)]
        public int Int;

        [NodePropertyPort(NAME_PORT_NAME, true, typeof(string), "NewChunk", true)]
        public string ChunkName;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(Chunk), null, true)]
        public Chunk Result;
        #endregion

        #region Constructors
        public IntToChunkNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "IntToChunk";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var number = GetPortValue(INT_PORT_NAME, Int);
            var chunkName = GetPortValue(NAME_PORT_NAME, ChunkName);
            Result = new Chunk
            {
                name = chunkName,
                content = number.ToString()
            };
            SetPortValue(RESULT_PORT_NAME, Result);
            _success = true;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}