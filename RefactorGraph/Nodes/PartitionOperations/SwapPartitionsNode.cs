﻿using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.SwapPartitions)]
    public class SwapPartitionsNode : RefactorNodeBase
    {
        #region Fields
        public const string A_PORT_NAME = "A";
        public const string B_PORT_NAME = "B";

        [NodePropertyPort(A_PORT_NAME, true, typeof(Partition), null, true, Serialized = false)]
        public Partition A;

        [NodePropertyPort(B_PORT_NAME, true, typeof(Partition), null, true, Serialized = false)]
        public Partition B;
        #endregion

        #region Constructors
        public SwapPartitionsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            A = GetPortValue<Partition>(A_PORT_NAME);
            B = GetPortValue<Partition>(B_PORT_NAME);
            if (A == null || B == null || A == B)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }

            Partition.Swap(A, B);
        }
        #endregion
    }
}