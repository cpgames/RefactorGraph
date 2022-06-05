using System;
using System.Windows.Media;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Logic, nodeType = RefactorNodeType.ThreeBus)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_1_PORT_NAME, "1", false)]
    [NodeFlowPort(OUTPUT_2_PORT_NAME, "2", false)]
    [NodeFlowPort(OUTPUT_3_PORT_NAME, "3", false)]
    public class ThreeBusNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_1_PORT_NAME = "Output1";
        public const string OUTPUT_2_PORT_NAME = "Output2";
        public const string OUTPUT_3_PORT_NAME = "Output3";
        #endregion

        #region Constructors
        public ThreeBusNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.ThreeBus)
        {
            Header = "3 Bus";
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
        }
        #endregion
    }
}