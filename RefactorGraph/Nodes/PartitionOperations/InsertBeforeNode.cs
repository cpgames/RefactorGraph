using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.InsertBefore)]
    public class InsertBeforeNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string DATA_PORT_NAME = "Data";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, true)]
        public Partition Partition;

        [NodePropertyPort(DATA_PORT_NAME, true, typeof(string), "", true)]
        public string Data;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Result;
        #endregion

        #region Constructors
        public InsertBeforeNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            Data = GetPortValue(DATA_PORT_NAME, Data);
            if (Partition != null && 
                Partition.prev != null /* can't insert before root */)
            {
                Result = new Partition
                {
                    Data = Data,
                    prev = Partition.prev,
                    next = Partition
                };
                Partition.prev.next = Result;
                Partition.prev = Result;
                SetPortValue(RESULT_PORT_NAME, Result);
                _success = true;
            }
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}