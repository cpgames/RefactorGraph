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

        public const string PREFIX_EMPTY_REGEX = @"^\s*\Z";
        public const string PREFIX_ASSIGNMENT_REGEX = @"^\s*\w[\w.\[\]\s]*\s*=\s*\Z";
        public const string PREFIX_OP_REGEX = @"\s*(?:\|\||&&|[,+\-*\/&\|\^])\s*\Z";
        public const string SUFFIX_OP_REGEX = @"\s*(?:\|\||&&|[,+\-*\/&\|\^])\s*";
        public const string SUFFIX_SEMICOLON_REGEX = @"\s*;";
        public const string SUFFIX_EMPTY_REGEX = @"\s*(?=^)";

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
            if (SmartTrim)
            {
                RemoveEmptyLines();
            }
            Partition.Remove();
        }

        private void RemovePrefix(out bool removedOp, out bool removedWhiteSpace)
        {
            removedOp = false;
            removedWhiteSpace = false;
            var op = Partition.PartitionByFirstRegexMatch(Partition.prev, PREFIX_OP_REGEX);
            if (op != null)
            {
                op.Remove();
                Partition.prev.Rasterize();
                removedOp = true;
            }
           
            var whitespace = Partition.PartitionByFirstRegexMatch(Partition.prev, PREFIX_EMPTY_REGEX);
            if (whitespace != null)
            {
                whitespace.Remove();
                Partition.prev.Rasterize();
                removedWhiteSpace = true;
            }
        }

        private void RemoveSuffix(bool removedOp, bool removedWhitespace)
        {
            if (!removedOp)
            {
                var op = Partition.PartitionByFirstRegexMatch(Partition.next, SUFFIX_OP_REGEX);
                if (op != null)
                {
                    op.Remove();
                    Partition.next.Rasterize();
                    return;
                }
            }
            var assignment = Partition.PartitionByFirstRegexMatch(Partition.prev, PREFIX_ASSIGNMENT_REGEX);
            if (assignment != null)
            {
                var suffix = Partition.PartitionByFirstRegexMatch(Partition.next, SUFFIX_SEMICOLON_REGEX);
                if (suffix != null)
                {
                    assignment.Remove();
                    suffix.Remove();
                    Partition.prev.Rasterize();
                    Partition.next.Rasterize();
                }
            }
            if (!removedWhitespace)
            {
                var whitespace = Partition.PartitionByFirstRegexMatch(Partition.next, SUFFIX_EMPTY_REGEX);
                if (whitespace != null)
                {
                    whitespace.Remove();
                    Partition.next.Rasterize();
                }
            }
        }

        private void RemoveEmptyLines()
        {
            var removedOp = false;
            var removedWhitespace = false;
            if (Partition.prev != null)
            {
                RemovePrefix(out removedOp, out removedWhitespace);
            }
            if (Partition.next != null)
            {
                RemoveSuffix(removedOp, removedWhitespace);
            }
        }
        #endregion
    }
}