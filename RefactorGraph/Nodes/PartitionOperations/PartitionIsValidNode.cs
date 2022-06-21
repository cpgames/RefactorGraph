using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionIsValid)]
    [NodeFlowPort(TRUE_PORT_NAME, TRUE_PORT_NAME, false)]
    [NodeFlowPort(FALSE_PORT_NAME, FALSE_PORT_NAME, false)]
    public class PartitionIsValidNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(bool), false, false)]
        public bool Result;
        #endregion

        #region Constructors
        public PartitionIsValidNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            Result = Source != null && !string.IsNullOrEmpty(Source.Data);
            SetPortValue(RESULT_PORT_NAME, Result);
            _success = true;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(Result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
        }
        #endregion
    }
}