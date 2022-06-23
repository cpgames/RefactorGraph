using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.RemoveFunctionCall)]
    public class RemoveFunctionCallNode : RefactorNodeBase
    {
        #region Fields
        public const string FUNCTION_CALL_PORT_NAME = "FunctionCall";

        public const string ASSIGNMENT_OPERATOR_REGEX = @"\s*=\s*\Z";
        public const string PREFIX_REGEX = @"[^\S\r\n]*\Z|[^\S\r\n]*\w[\w.\[\]\s]*\s*=\s*\Z";
        public const string SUFFIX_REGEX = @"\s*[;,+-].*\R*|\s*&&.*\R*|\s*\|\|.*\R*|\s*\R*";

        [NodePropertyPort(FUNCTION_CALL_PORT_NAME, true, typeof(Partition), null, true)]
        public Partition FunctionCall;
        #endregion

        #region Constructors
        public RemoveFunctionCallNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            FunctionCall = GetPortValue<Partition>(FUNCTION_CALL_PORT_NAME);
            if (FunctionCall != null &&
                !FunctionCall.IsRoot /* can't remove root */)
            {
                RemoveEmptyLines();
                FunctionCall.Remove();
            }
        }
        
        private void RemoveEmptyLines()
        {
            var cur = FunctionCall.prev;
            if (cur != null && !cur.IsPartitioned)
            {
                cur = cur.PartitionByFirstRegexMatch(PREFIX_REGEX, PcreOptions.MultiLine);
                if (cur != null)
                {
                    cur.Remove();
                }
            }
            cur = FunctionCall.next;
            if (cur != null && !cur.IsPartitioned)
            {
                cur = cur.PartitionByFirstRegexMatch(SUFFIX_REGEX, PcreOptions.MultiLine);
                if (cur != null)
                {
                    cur.Remove();
                }
            }
        }
        #endregion
    }
}