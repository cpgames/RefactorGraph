using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByIfElse)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_IF_ELSE_BLOCK_PORT_NAME, "LoopIfElse", false)]
    [NodeFlowPort(LOOP_EVERY_CLAUSE_PORT_NAME, "LoopClause", false)]
    public class PartitionByIfElseNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_IF_ELSE_BLOCK_PORT_NAME = "LoopIfElse";
        public const string LOOP_EVERY_CLAUSE_PORT_NAME = "LoopClause";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string SOURCE_PORT_NAME = "Source";

        public const string CONDITIONS_FILTER_PORT_NAME = "ConditionsFilterRegex";
        public const string STATEMENT_FILTER_PORT_NAME = "StatementFilterRegex";

        public const string IF_ELSE_BLOCK = "IfElseBlock";
        public const string CONDITION_PORT_NAME = "Condition";
        public const string STATEMENT_PORT_NAME = "Statement";

        private const string IF_ELSE_BLOCK_REGEX = @"else\s*(*SKIP)(*F)|" + // do not start if statement starts with "else"
            @"^\s*\Kif\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})" + // if (condition) { body }
            @"(?:\s*else\s*if\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*}))*" + // else if (condition) { body }
            @"(?:\s*else\s*\s*({(?:[^{}]|(?-1))*}))*"; // else { body }
        private const string CLAUSE_TYPE_REGEX = @"\s*(?:if|else|else\s*if)";
        private const string CONDITION_BLOCK_REGEX = @"\((?:[^()]|(?R))*\)";
        private const string CONDITION_REGEX = @"\(\s*\K[\s\S]*[^\s](?=\s*\))";
        private const string STATEMENT_BLOCK_REGEX = @"{(?:[^{}]|(?R))*}";
        private const string STATEMENT_REGEX = @"{\s*\K[\s\S]*[^\s](?=\s*})";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(CONDITIONS_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ConditionsFilterRegex;

        [NodePropertyPort(STATEMENT_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string BodyFilterRegex;

        [NodePropertyPort(IF_ELSE_BLOCK, false, typeof(Partition), null, false)]
        public Partition IfElseBlock;

        [NodePropertyPort(CONDITION_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Condition;

        [NodePropertyPort(STATEMENT_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Statement;

        private bool _somethingReturned;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public PartitionByIfElseNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _somethingReturned = false;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Partition.IsValidAndNotPartitioned(Source))
            {
                PartitionIfElse(Source);
            }
        }

        private void PartitionIfElse(Partition cur)
        {
            var ifElseBlock = cur.PartitionByFirstRegexMatch(IF_ELSE_BLOCK_REGEX, PcreOptions.MultiLine);
            if (ifElseBlock == null)
            {
                return;
            }
            if (!PartitionClauses(ifElseBlock))
            {
                return;
            }
            IfElseBlock = ifElseBlock;
            SetPortValue(IF_ELSE_BLOCK, IfElseBlock);
            if (_somethingReturned)
            {
                ExecutePort(LOOP_IF_ELSE_BLOCK_PORT_NAME);
            }
            cur = ifElseBlock.next;
            if (cur != null)
            {
                PartitionIfElse(cur);
            }
        }

        private bool PartitionClauses(Partition cur)
        {
            var finalClause = false;
            cur = cur.PartitionByFirstRegexMatch(CLAUSE_TYPE_REGEX, PcreOptions.MultiLine);
            if (cur == null)
            {
                return false;
            }
            if (PcreRegex.IsMatch(cur.Data, @"\s*else\s*\Z", PcreOptions.MultiLine))
            {
                finalClause = true;
            }
            cur = cur.next;
            cur = cur.PartitionByFirstRegexMatch(CONDITION_BLOCK_REGEX, PcreOptions.MultiLine);
            if (cur == null)
            {
                return false;
            }
            Condition = cur.PartitionByFirstRegexMatch(CONDITION_REGEX, PcreOptions.MultiLine);
            SetPortValue(CONDITION_PORT_NAME, Condition);
            if (Condition == null)
            {
                return false;
            }
            cur = cur.next;
            cur = cur.PartitionByFirstRegexMatch(STATEMENT_BLOCK_REGEX, PcreOptions.MultiLine);
            if (cur == null)
            {
                return false;
            }
            Statement = cur.PartitionByFirstRegexMatch(STATEMENT_REGEX, PcreOptions.MultiLine);
            SetPortValue(STATEMENT_PORT_NAME, Statement);
            if (Statement == null)
            {
                return false;
            }
            if (ApplyFilter())
            {
                ExecutePort(LOOP_EVERY_CLAUSE_PORT_NAME);
                _somethingReturned = true;
            }
            PartitionIfElse(Statement);
            if (finalClause)
            {
                return true;
            }
            cur = cur.next;
            if (cur == null)
            {
                return true;
            }
            return PartitionClauses(cur);
        }

        private bool ApplyFilter()
        {
            ConditionsFilterRegex = GetPortValue<string>(CONDITIONS_FILTER_PORT_NAME);
            if (!string.IsNullOrEmpty(ConditionsFilterRegex))
            {
                if (!PcreRegex.IsMatch(Condition.Data, ConditionsFilterRegex, PcreOptions.MultiLine))
                {
                    return false;
                }
            }
            BodyFilterRegex = GetPortValue(STATEMENT_FILTER_PORT_NAME, BodyFilterRegex);
            if (!string.IsNullOrEmpty(BodyFilterRegex))
            {
                if (!PcreRegex.IsMatch(Statement.Data, BodyFilterRegex))
                {
                    return false;
                }
            }
            return true;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(COMPLETED_PORT_NAME);
        }
        #endregion
    }
}