﻿using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Variables
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Variables, RefactorNodeType.Partition)]
    public class PartitionNode : VariableNode<Partition>
    {
        #region Fields
        public const string DATA_PORT_NAME = "Data";
        public const string PREV_PORT_NAME = "Prev";
        public const string NEXT_PORT_NAME = "Next";
        public const string INNER_PORT_NAME = "Inner";
        #endregion

        #region Properties
        protected override bool HasEditor => true;
        protected override string DisplayNameOut => "PartitionOperations";

        [NodePropertyPort(DATA_PORT_NAME, false, typeof(string), "", false)]
        public string Data
        {
            get => Value != null ? Value.Data : string.Empty;
            set
            {
                if (Value != null)
                {
                    Value.Data = value;
                }
            }
        }

        [NodePropertyPort(PREV_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Prev
        {
            get => Value.prev;
            set => Value.prev = value;
        }
        [NodePropertyPort(NEXT_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Next
        {
            get => Value.next;
            set => Value.next = value;
        }
        [NodePropertyPort(INNER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Inner
        {
            get => Value.inner;
            set => Value.inner = value;
        }
        #endregion

        #region Constructors
        public PartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion
    }
}