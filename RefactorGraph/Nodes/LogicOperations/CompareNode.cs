using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.LogicOperations
{
    public enum CompareAlgorithm
    {
        Equals,
        LessThan,
        LessThanOrEquals,
        GreaterThan,
        GreaterThanOrEquals
    }

    [Node]
    [RefactorNode(RefactorNodeGroup.LogicOperations, RefactorNodeType.Compare)]
    public class CompareNode : DynamicRefactorNodeBase
    {
        #region Fields
        public const string ALGORITHM_PORT_NAME = "Algorithm";
        public const string A_PORT_NAME = "A";
        public const string B_PORT_NAME = "B";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";

        [NodePropertyPort(ALGORITHM_PORT_NAME, false, typeof(CompareAlgorithm), CompareAlgorithm.Equals, true)]
        public CompareAlgorithm Algorithm;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public CompareNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddElementPort(A_PORT_NAME, true);
            AddElementPort(B_PORT_NAME, true);
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            ElementType = GetPortValue(ELEMENT_TYPE_PORT_NAME, ElementType);
            Algorithm = GetPortValue(ALGORITHM_PORT_NAME, Algorithm);
            var result = false;
            switch (ElementType)
            {
                case CollectionType.String:
                    result = CompareStrings();
                    break;
                case CollectionType.Int:
                    result = CompareInts();
                    break;
                case CollectionType.Partition:
                    result = ComparePartitions();
                    break;
                case CollectionType.Bool:
                    result = CompareBools();
                    break;
            }
            ExecutePort(result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
        }

        private bool CompareStrings()
        {
            var a = GetPortValue<string>(A_PORT_NAME);
            var b = GetPortValue<string>(B_PORT_NAME);
            var r = string.Compare(a, b, StringComparison.Ordinal);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    return r == 0;
                case CompareAlgorithm.LessThan:
                    return r < 0;
                case CompareAlgorithm.LessThanOrEquals:
                    return r <= 0;
                case CompareAlgorithm.GreaterThan:
                    return r > 0;
                case CompareAlgorithm.GreaterThanOrEquals:
                    return r >= 0;
            }
            return false;
        }

        private bool CompareInts()
        {
            var a = GetPortValue<int>(A_PORT_NAME);
            var b = GetPortValue<int>(B_PORT_NAME);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    return a == b;
                case CompareAlgorithm.LessThan:
                    return a < b;
                case CompareAlgorithm.LessThanOrEquals:
                    return a <= b;
                case CompareAlgorithm.GreaterThan:
                    return a > b;
                case CompareAlgorithm.GreaterThanOrEquals:
                    return a >= b;
            }
            return false;
        }

        private bool ComparePartitions()
        {
            var a = GetPortValue<Partition>(A_PORT_NAME);
            var b = GetPortValue<Partition>(B_PORT_NAME);
            var r = string.Compare(a.Data, b.Data, StringComparison.Ordinal);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    return r == 0;
                case CompareAlgorithm.LessThan:
                    return r < 0;
                case CompareAlgorithm.LessThanOrEquals:
                    return r <= 0;
                case CompareAlgorithm.GreaterThan:
                    return r > 0;
                case CompareAlgorithm.GreaterThanOrEquals:
                    return r >= 0;
            }
            return false;
        }

        private bool CompareBools()
        {
            var a = GetPortValue<bool>(A_PORT_NAME);
            var b = GetPortValue<bool>(B_PORT_NAME);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    return a == b;
                case CompareAlgorithm.LessThan:
                    return !a && b;
                case CompareAlgorithm.LessThanOrEquals:
                    return (!a && b) || a == b;
                case CompareAlgorithm.GreaterThan:
                    return a && !b;
                case CompareAlgorithm.GreaterThanOrEquals:
                    return (a && !b) || a == b;
            }
            return false;
        }
        #endregion
    }
}