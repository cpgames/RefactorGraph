using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetPreviousPartition)]
    public class GetPreviousPartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";
        public const string PREVIOUS_PORT_NAME = "Previous";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(PREVIOUS_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Previous;
        #endregion

        #region Properties
        public override bool Success => Previous != null;
        #endregion

        #region Constructors
        public GetPreviousPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Previous = null;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null)
            {
                Previous = Partition.GetPrevious(Source);
            }
        }
        #endregion
    }
}