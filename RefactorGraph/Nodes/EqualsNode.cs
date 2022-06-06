using System;
using System.Windows.Media;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Math, nodeType = RefactorNodeType.Equals)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(TRUE_PORT_NAME, TRUE_PORT_NAME, false)]
    [NodeFlowPort(FALSE_PORT_NAME, FALSE_PORT_NAME, false)]
    public class EqualsNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";
        public const string A_PORT_NAME = "A";
        public const string B_PORT_NAME = "B";

        private bool _equals;

        [NodePropertyPort(A_PORT_NAME, true, typeof(int), 0, true)]
        public int A;

        [NodePropertyPort(B_PORT_NAME, true, typeof(int), 0, true)]
        public int B;
        #endregion

        #region Constructors
        public EqualsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "A = B";
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
            _equals = a == b;
            _success = true;
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            if (_equals)
            {
                ExecutePort(TRUE_PORT_NAME);
            }
            else
            {
                ExecutePort(FALSE_PORT_NAME);
            }
        }
        #endregion
    }
}