using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.RemovePartition)]
    public class RemovePartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string TRIM_PORT_NAME = "SmartTrim";

        public const string PREFIX_EMPTY_REGEX = @"[^\S\n]*[\n]?\Z";
        public const string PREFIX_ASSIGNMENT_REGEX = @"[^\S\n]*[\n]?\w[\w.\[\]\s]*\s*=\s*\Z";
        public const string PREFIX_OP_REGEX = @"\s*(?:\|\||&&|[,+\-*\/&\|\^])\s*\Z";
        public const string SUFFIX_OP_REGEX = @"\s*(?:\|\||&&|[,+\-*\/&\|\^])";
        public const string SUFFIX_SEMICOLON_REGEX = @"\s*;";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, true, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(TRIM_PORT_NAME, true, typeof(bool), false, true)]
        public bool SmartTrim;
        #endregion

        #region Constructors
        public RemovePartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            SmartTrim = GetPortValue(TRIM_PORT_NAME, SmartTrim);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            RemoveEmptyLines();
            Partition.Remove();
        }

        private void RemovePrefix(out bool removedOp)
        {
            removedOp = false;
            var op = Partition.prev.PartitionByFirstRegexMatch(PREFIX_OP_REGEX);
            if (op != null)
            {
                op.Remove();
                removedOp = true;
                return;
            }
            var assignment = Partition.prev.PartitionByFirstRegexMatch(PREFIX_ASSIGNMENT_REGEX);
            if (assignment != null)
            {
                assignment.Remove();
                return;
            }
            var regular = Partition.prev.PartitionByFirstRegexMatch(PREFIX_EMPTY_REGEX);
            if (regular != null)
            {
                regular.Remove();
            }
        }

        private void RemoveSuffix(bool removedOp)
        {
            if (!removedOp)
            {
                var op = Partition.next.PartitionByFirstRegexMatch(SUFFIX_OP_REGEX);
                if (op != null)
                {
                    op.Remove();
                    return;
                }
            }
            var suffix = Partition.next.PartitionByFirstRegexMatch(SUFFIX_SEMICOLON_REGEX);
            if (suffix != null)
            {
                suffix.Remove();
            }
        }

        private void RemoveEmptyLines()
        {
            var removedOp = false;
            if (Partition.prev != null)
            {
                RemovePrefix(out removedOp);
            }
            if (Partition.next != null)
            {
                RemoveSuffix(removedOp);
            }
        }
        #endregion
    }
}