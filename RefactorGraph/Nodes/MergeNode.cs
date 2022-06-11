using System;
using System.Windows.Media;
using NodeGraph.Model;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.Merge)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class MergeNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string SOURCE_PORT_NAME = "Source";
        public const string TARGET_PORT_NAME = "Target";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Source\n[Chunk]")]
        public Chunk Source;

        [NodePropertyPort(TARGET_PORT_NAME, true, typeof(Chunk), null, false, DisplayName = "Target\n[Chunk]")]
        public Chunk Target;
        #endregion

        #region Constructors
        public MergeNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HeaderBackgroundColor = Brushes.DarkBlue;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            Source = GetPortValue<Chunk>(SOURCE_PORT_NAME);
            Target = GetPortValue<Chunk>(TARGET_PORT_NAME);
            if (Source == null || Target == null)
            {
                return;
            }
            if (Target.content.Length <= Source.index + Source.length)
            {
                return;
            }
            Target.content =
                Target.content.Substring(0, Source.index) +
                Source.content +
                Target.content.Substring(Source.index);
            SetPortValue(TARGET_PORT_NAME, Target);
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