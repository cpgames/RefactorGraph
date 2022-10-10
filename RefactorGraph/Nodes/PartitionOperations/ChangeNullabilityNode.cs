using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.ChangeNullability)]
    public class ChangeNullabilityNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string NULLABLE_PORT_NAME = "Nullable";
        public const string NULLABLE_MODIFIED_PORT_NAME = "NullableModified";

        private const string NULLABLE_PREFIX_REGEX = @"\w+";

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(NULLABLE_PORT_NAME, true, typeof(NullableModifier), NullableModifier.Nullable, true)]
        public NullableModifier Nullable;

        [NodePropertyPort(NULLABLE_MODIFIED_PORT_NAME, false, typeof(bool), false, false, Serialized = false)]
        public bool NullableModified;
        #endregion

        #region Constructors
        public ChangeNullabilityNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            NullableModified = false;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            Nullable = GetPortValue(NULLABLE_PORT_NAME, Nullable);
            if (Partition?.data == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            Partition.Rasterize();
            ChangeNullability();
        }

        private void ChangeNullability()
        {
            var match = PcreRegex.Match(Partition.data, NULLABLE_PREFIX_REGEX);
            if (!match.Success || match.Length == 0)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            var indexNext = match.Index + match.Length;
            var isNullable = indexNext < Partition.data.Length && Partition.data[indexNext] == '?';
            switch (Nullable)
            {
                case NullableModifier.Nullable:
                    if (isNullable)
                    {
                        return;
                    }
                    Partition.data = Partition.data.Insert(indexNext, "?");
                    NullableModified = true;
                    break;
                case NullableModifier.NonNullable:
                    if (!isNullable)
                    {
                        return;
                    }
                    Partition.data = Partition.data.Remove(indexNext, 1);
                    NullableModified = true;
                    break;
                case NullableModifier.Toggle:
                    Partition.data = isNullable ?
                        Partition.data.Remove(indexNext, 1) :
                        Partition.data.Insert(indexNext, "?");
                    NullableModified = true;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        #endregion
    }
}