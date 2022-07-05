using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.LogicOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.LogicOperations, RefactorNodeType.Multiply)]
    public class MultiplyNode : RefactorNodeBase
    {
        #region Fields
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
        public MultiplyNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "A * B";
        }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            var a = GetPortValue(A_PORT_NAME, A);
            var b = GetPortValue(B_PORT_NAME, B);
            Result = a * b;
        }
        #endregion
    }
}