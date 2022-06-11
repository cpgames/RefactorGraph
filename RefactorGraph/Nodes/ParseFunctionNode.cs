using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.ParseFunction)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class ParseFunctionNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string SOURCE_PORT_NAME = "Source";
        public const string FUNCTION_NAME_PORT_NAME = "FunctionName";
        public const string FUNCTION_PARAMETERS_PORT_NAME = "FunctionParameters";

        private const string FUNCTION_NAME_REGEX = @"[^\s]*(?=\()";
        private const string FUNCTION_BODY_REGEX = @"(?<=\().*(?=\))";
        private const string FUNCTION_PARAMS_REGEX = @"[^,\s*][a-zA-Z\s<>.()0-9]*";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, false)]
        public Chunk Source;

        [NodePropertyPort(SOURCE_PORT_NAME, false, typeof(Chunk), null, false)]
        public Chunk FunctionName;

        [NodePropertyPort(FUNCTION_PARAMETERS_PORT_NAME, false, typeof(ChunkCollection), null, true)]
        public ChunkCollection FunctionParameters;
        #endregion

        #region Constructors
        public ParseFunctionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkMagenta;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            if (Source != null &&
                !string.IsNullOrEmpty(Source.content) &&
                SplitChunk())
            {
                SetPortValue(FUNCTION_NAME_PORT_NAME, FunctionName);
                SetPortValue(FUNCTION_PARAMETERS_PORT_NAME, FunctionParameters);
                _success = true;
            }
        }

        private bool SplitChunk()
        {
            var offset = Source.index + Source.length;
            var functionNameMatch = Regex.Match(Source.content, FUNCTION_NAME_REGEX, RegexOptions.Multiline);
            if (!functionNameMatch.Success)
            {
                return false;
            }
            FunctionName = new Chunk
            {
                content = functionNameMatch.Value,
                index = functionNameMatch.Index + offset,
                length = functionNameMatch.Length
            };
            var functionBodyMatch = Regex.Match(Source.content, FUNCTION_BODY_REGEX, RegexOptions.Multiline);
            if (!functionBodyMatch.Success)
            {
                return false;
            }
            offset += functionBodyMatch.Index;
            var functionParamsMatches = Regex.Matches(functionBodyMatch.Value, FUNCTION_PARAMS_REGEX, RegexOptions.Multiline);
            FunctionParameters = new ChunkCollection();
            foreach (Match functionParamsMatch in functionParamsMatches)
            {
                var paramStr = functionParamsMatch.Value.Trim();
                if (paramStr.Length == 0)
                {
                    var paramChunk = new Chunk
                    {
                        content = paramStr,
                        index = functionNameMatch.Index + offset,
                        length = functionBodyMatch.Length
                    };
                    FunctionParameters.Add(paramChunk);
                }
            }
            return true;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}