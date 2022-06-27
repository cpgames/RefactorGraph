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
        public const string CLAUSE_BLOCK = "ClauseBlock";
        public const string CONDITION_PORT_NAME = "Condition";
        public const string STATEMENT_PORT_NAME = "Statement";

        private const string IF_ELSE_BLOCK_REGEX = @"else\s*(*SKIP)(*F)|" + // do not start if clause starts with "else"
            @"^\s*\Kif\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})" + // if (condition) { body }
            @"(?:\s*else\s*if\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*}))*" + // else if (condition) { body }
            @"(?:\s*else\s*\s*({(?:[^{}]|(?-1))*}))*"; // else { body }
        private const string CLAUSE_BLOCK_REGEX = @"^\s*\Kif\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})|" +
            @"^\s*\Kelse\s*if\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})|" +
            @"^\s*\K\s*else\s*\s*({(?:[^{}]|(?-1))*})";
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
        public string StatementFilterRegex;

        [NodePropertyPort(IF_ELSE_BLOCK, false, typeof(Partition), null, false)]
        public Partition IfElseBlock;

        [NodePropertyPort(CLAUSE_BLOCK, false, typeof(Partition), null, false)]
        public Partition ClauseBlock;

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
            PartitionClauses(ifElseBlock);
            if (_somethingReturned)
            {
                IfElseBlock = ifElseBlock;
                ExecutePort(LOOP_IF_ELSE_BLOCK_PORT_NAME);
                _somethingReturned = false;
            }
            cur = ifElseBlock.next;
            if (cur != null)
            {
                PartitionIfElse(cur);
            }
        }

        private void PartitionClauses(Partition cur)
        {
            var finalClause = false;
            var clauseBlock = cur.PartitionByFirstRegexMatch(CLAUSE_BLOCK_REGEX, PcreOptions.MultiLine);
            if (clauseBlock == null)
            {
                return;
            }
            var clauseType = clauseBlock.PartitionByFirstRegexMatch(CLAUSE_TYPE_REGEX, PcreOptions.MultiLine);
            if (PcreRegex.IsMatch(clauseType.Data, @"\s*else\s*\Z", PcreOptions.MultiLine))
            {
                finalClause = true;
            }
            cur = clauseType.next;
            cur = cur.PartitionByFirstRegexMatch(CONDITION_BLOCK_REGEX, PcreOptions.MultiLine);
            if (cur == null)
            {
                return;
            }
            var condition = cur.PartitionByFirstRegexMatch(CONDITION_REGEX, PcreOptions.MultiLine);
            if (condition == null)
            {
                return;
            }
            cur = cur.next;
            cur = cur.PartitionByFirstRegexMatch(STATEMENT_BLOCK_REGEX, PcreOptions.MultiLine);
            if (cur == null)
            {
                return;
            }
            var statement = cur.PartitionByFirstRegexMatch(STATEMENT_REGEX, PcreOptions.MultiLine);
            if (statement == null)
            {
                return;
            }
            PartitionIfElse(statement);
            if (ApplyFilter(condition, statement))
            {
                ClauseBlock = clauseBlock;
                Condition = condition;
                Statement = statement;
                ExecutePort(LOOP_EVERY_CLAUSE_PORT_NAME);
                _somethingReturned = true;
            }
            if (finalClause)
            {
                return;
            }
            cur = clauseBlock.next;
            if (cur == null)
            {
                return;
            }
            PartitionClauses(cur);
        }

        private bool ApplyFilter(Partition condition, Partition statement)
        {
            ConditionsFilterRegex = GetPortValue(CONDITIONS_FILTER_PORT_NAME, ConditionsFilterRegex);
            if (!string.IsNullOrEmpty(ConditionsFilterRegex))
            {
                if (!PcreRegex.IsMatch(condition.Data, ConditionsFilterRegex, PcreOptions.MultiLine))
                {
                    return false;
                }
            }
            StatementFilterRegex = GetPortValue(STATEMENT_FILTER_PORT_NAME, StatementFilterRegex);
            if (!string.IsNullOrEmpty(StatementFilterRegex))
            {
                if (!PcreRegex.IsMatch(statement.Data, StatementFilterRegex))
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