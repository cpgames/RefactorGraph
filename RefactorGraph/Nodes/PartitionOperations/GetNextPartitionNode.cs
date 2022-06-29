using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetNextPartition)]
    public class GetNextPartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";
        public const string NEXT_PORT_NAME = "Next";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(NEXT_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Next;
        #endregion

        #region Properties
        public override bool Success => Next != null;
        #endregion

        #region Constructors
        public GetNextPartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Next = null;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null && Partition.IsValid(Source.next))
            {
                Next = Source.next;
            }
        }
        #endregion
    }
}