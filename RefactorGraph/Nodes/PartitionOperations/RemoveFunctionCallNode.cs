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
        public const string ASSIGNMENT_REGEX = @"^.*[\/].*\R(*SKIP)(*F)|\s*\w[\w\s*\[\]]*\Z";
        public const string SUFFIX_REGEX = @"\s*[;,+-].*|\s*&&.*|\s*\|\|.*";

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
                if (RemovePrefix(FunctionCall.prev))
                {
                    RemoveSuffix(FunctionCall.next);
                }

                FunctionCall.Remove();
                _success = true;
            }
        }

        private bool RemovePrefix(Partition cur)
        {
            if (cur.IsRoot || cur.IsPartitioned)
            {
                return true;
            }
            var assignmentOperator = cur.PartitionByFirstRegexMatch(ASSIGNMENT_OPERATOR_REGEX, PcreOptions.MultiLine);
            if (assignmentOperator != null && !assignmentOperator.prev.IsRoot)
            {
                var assignment = assignmentOperator.prev.PartitionByFirstRegexMatch(ASSIGNMENT_REGEX, PcreOptions.MultiLine);
                assignment.Remove();
                assignmentOperator.Remove();
                return true;
            }
            return true;
        }

        private void RemoveSuffix(Partition cur)
        {
            if (cur == null)
            {
                return;
            }
            var suffix = cur.PartitionByFirstRegexMatch(SUFFIX_REGEX, PcreOptions.MultiLine);
            if (suffix != null)
            {
                suffix.Remove();
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