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
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(ALGORITHM_PORT_NAME, false, typeof(CompareAlgorithm), CompareAlgorithm.Equals, true)]
        public CompareAlgorithm Algorithm;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(bool), false, false)]
        public bool Result;
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
            switch (ElementType)
            {
                case CollectionType.String:
                    CompareStrings();
                    break;
                case CollectionType.Int:
                    CompareInts();
                    break;
                case CollectionType.Partition:
                    ComparePartitions();
                    break;
                case CollectionType.Bool:
                    CompareBools();
                    break;
            }

            SetPortValue(RESULT_PORT_NAME, Result);
            _success = true;
        }

        private void CompareStrings()
        {
            var a = GetPortValue<string>(A_PORT_NAME);
            var b = GetPortValue<string>(B_PORT_NAME);
            var r = string.Compare(a, b, StringComparison.Ordinal);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    Result = r == 0;
                    break;
                case CompareAlgorithm.LessThan:
                    Result = r < 0;
                    break;
                case CompareAlgorithm.LessThanOrEquals:
                    Result = r <= 0;
                    break;
                case CompareAlgorithm.GreaterThan:
                    Result = r > 0;
                    break;
                case CompareAlgorithm.GreaterThanOrEquals:
                    Result = r >= 0;
                    break;
            }
        }

        private void CompareInts()
        {
            var a = GetPortValue<int>(A_PORT_NAME);
            var b = GetPortValue<int>(B_PORT_NAME);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    Result = a == b;
                    break;
                case CompareAlgorithm.LessThan:
                    Result = a < b;
                    break;
                case CompareAlgorithm.LessThanOrEquals:
                    Result = a <= b;
                    break;
                case CompareAlgorithm.GreaterThan:
                    Result = a > b;
                    break;
                case CompareAlgorithm.GreaterThanOrEquals:
                    Result = a >= b;
                    break;
            }
        }

        private void ComparePartitions()
        {
            var a = GetPortValue<Partition>(A_PORT_NAME);
            var b = GetPortValue<Partition>(B_PORT_NAME);
            var r = string.Compare(a.Data, b.Data, StringComparison.Ordinal);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    Result = r == 0;
                    break;
                case CompareAlgorithm.LessThan:
                    Result = r < 0;
                    break;
                case CompareAlgorithm.LessThanOrEquals:
                    Result = r <= 0;
                    break;
                case CompareAlgorithm.GreaterThan:
                    Result = r > 0;
                    break;
                case CompareAlgorithm.GreaterThanOrEquals:
                    Result = r >= 0;
                    break;
            }
        }

        private void CompareBools()
        {
            var a = GetPortValue<bool>(A_PORT_NAME);
            var b = GetPortValue<bool>(B_PORT_NAME);
            switch (Algorithm)
            {
                case CompareAlgorithm.Equals:
                    Result = a == b;
                    break;
                case CompareAlgorithm.LessThan:
                    Result = !a && b;
                    break;
                case CompareAlgorithm.LessThanOrEquals:
                    Result = (!a && b) || a == b;
                    break;
                case CompareAlgorithm.GreaterThan:
                    Result = a && !b;
                    break;
                case CompareAlgorithm.GreaterThanOrEquals:
                    Result = (a && !b) || a == b;
                    break;
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