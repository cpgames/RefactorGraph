using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByIfElse)]
    public class PartitionByIfElseNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_CLAUSE_PORT_NAME = "LoopClause";
        public const string PARTITION_PORT_NAME = "Partition";

        public const string CLAUSE_CONDITION_FILTER_PORT_NAME = "ClauseConditionFilter";

        public const string IF_ELSE = "IfElse";
        public const string CLAUSE = "Clause";
        public const string CLAUSE_CONDITION_PORT_NAME = "ClauseCondition";
        public const string CLAUSE_BODY_PORT_NAME = "ClauseBody";

        private const string IF_ELSE_REGEX = @"else\s*(*SKIP)(*F)|" + // do not start if clause starts with "else"
            @"^\s*\Kif\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})" + // if (condition) { body }
            @"(?:\s*else\s*if\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*}))*" + // else if (condition) { body }
            @"(?:\s*else\s*\s*({(?:[^{}]|(?-1))*}))*"; // else { body }
        private const string CLAUSE_REGEX = @"^\s*\Kif\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})|" +
            @"^\s*\Kelse\s*if\s*(\((?:[^()]++|(?-1))*\))\s*({(?:[^{}]|(?-1))*})|" +
            @"^\s*\K\s*else\s*\s*({(?:[^{}]|(?-1))*})";
        private const string CLAUSE_TYPE_REGEX = @"\s*(?:if|else|else\s*if)";
        private const string CONDITION_BLOCK_REGEX = @"\((?:[^()]|(?R))*\)";
        private const string CONDITION_REGEX = @"\(\s*\K[\s\S]*[^\s](?=\s*\))";
        private const string BODY_BLOCK_REGEX = @"{(?:[^{}]|(?R))*}";
        private const string BODY_REGEX = @"{\s*\K[\s\S]*[^\s](?=\s*})";

        private static readonly string[] TYPE_CONDITION_BODY = { CLAUSE_TYPE_REGEX, CONDITION_BLOCK_REGEX, BODY_BLOCK_REGEX };

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(CLAUSE_CONDITION_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ClauseConditionFilter;

        [NodePropertyPort(IF_ELSE, false, typeof(Partition), null, false, Serialized = false)]
        public Partition IfElse;

        [NodePropertyPort(CLAUSE, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Clause;

        [NodePropertyPort(CLAUSE_CONDITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition ClauseCondition;

        [NodePropertyPort(CLAUSE_BODY_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition ClauseBody;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByIfElseNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();
            
            CreateOutputFlowPort(LOOP_CLAUSE_PORT_NAME);
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
            PartitionIfElses(Partition);
        }

        private void PartitionIfElses(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, IF_ELSE_REGEX);
            foreach (var p in partitions)
            {
                IfElse = p;
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
                PartitionClauses(p);
            }
        }

        private void PartitionClauses(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, CLAUSE_REGEX);
            var pBodies = new List<Partition>();
            foreach (var p in partitions)
            {
                var type_condition_body = Partition.PartitionByRegexMatch(p, TYPE_CONDITION_BODY);
                Clause = p;
                ClauseCondition = Partition.PartitionByFirstRegexMatch(type_condition_body[1], CONDITION_REGEX);
                ClauseBody = Partition.PartitionByFirstRegexMatch(type_condition_body[2], BODY_REGEX);
                if (ApplyFilter())
                {
                    var executionState = ExecutePort(LOOP_CLAUSE_PORT_NAME);
                    if (executionState == ExecutionState.Failed)
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                }
                if (ExecutionState != ExecutionState.Skipped)
                {
                    pBodies.Add(ClauseBody);
                }
            }
            foreach (var p in pBodies)
            {
                PartitionIfElses(p);
            }
        }

        private bool ApplyFilter()
        {
            ClauseConditionFilter = GetPortValue(CLAUSE_CONDITION_FILTER_PORT_NAME, ClauseConditionFilter);
            if (!Partition.IsMatch(ClauseCondition, ClauseConditionFilter))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}