using System;
using System.Windows.Media;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Logic, nodeType = RefactorNodeType.FourBus)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_1_PORT_NAME, "1", false)]
    [NodeFlowPort(OUTPUT_2_PORT_NAME, "2", false)]
    [NodeFlowPort(OUTPUT_3_PORT_NAME, "3", false)]
    [NodeFlowPort(OUTPUT_4_PORT_NAME, "4", false)]
    public class FourBusNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_1_PORT_NAME = "Output1";
        public const string OUTPUT_2_PORT_NAME = "Output2";
        public const string OUTPUT_3_PORT_NAME = "Output3";
        public const string OUTPUT_4_PORT_NAME = "Output4";
        #endregion

        #region Constructors
        public FourBusNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "4 Bus";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);
            _success = true;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_1_PORT_NAME);
            ExecutePort(OUTPUT_2_PORT_NAME);
            ExecutePort(OUTPUT_3_PORT_NAME);
            ExecutePort(OUTPUT_4_PORT_NAME);
        }
        #endregion
    }
}