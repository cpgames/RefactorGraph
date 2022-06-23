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

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public PartitionIsValidNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            var result = Source != null && !string.IsNullOrEmpty(Source.Data);
            ExecutePort(result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
        }
        #endregion
    }
}