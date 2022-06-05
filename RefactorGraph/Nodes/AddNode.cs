using System;
using System.Windows.Media;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Math, nodeType = RefactorNodeType.Add)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class AddNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string A_PORT_NAME = "A";
        public const string B_PORT_NAME = "B";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(A_PORT_NAME, true, typeof(int), 0, true)]
        public int A;

        [NodePropertyPort(B_PORT_NAME, true, typeof(int), 0, true)]
        public int B;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(int), 0, true)]
        public int Result;
        #endregion

        #region Constructors
        public AddNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.Add)
        {
            Header = "A + B";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var a = GetPortValue(A_PORT_NAME, A);
            var b = GetPortValue(B_PORT_NAME, B);
            Result = a + b;
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