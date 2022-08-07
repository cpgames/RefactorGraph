using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.GetLeadingSpacing)]
    public class GetLeadingSpacingNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string SPACING_PORT_NAME = "Spacing";

        public const string LEADING_SPACING_REGEX = @"\n?[^\S\n]*\Z";
        
        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(SPACING_PORT_NAME, false, typeof(string), "", false)]
        public string Spacing;
        #endregion

        #region Constructors
        public GetLeadingSpacingNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Spacing = string.Empty;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            var prev = Partition.GetPrev();
            if (prev != null)
            {
                var match = PcreRegex.Match(prev.RasterizedData, LEADING_SPACING_REGEX);
                if (match.Success)
                {
                    Spacing = match.Value;
                }
            }
        }
        #endregion
    }
}