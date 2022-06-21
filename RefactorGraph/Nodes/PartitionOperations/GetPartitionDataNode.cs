﻿using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetPartitionData)]
    public class GetPartitionDataNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string DATA_PORT_NAME = "Data";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Partition;

        [NodePropertyPort(DATA_PORT_NAME, false, typeof(string), "", false)]
        public string Data;
        #endregion

        #region Constructors
        public GetPartitionDataNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition != null)
            {
                Data = Partition.Data;
                SetPortValue(DATA_PORT_NAME, Data);
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