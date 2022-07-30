using System;
using System.Linq;
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
    public class CompareNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string ALGORITHM_PORT_NAME = "Algorithm";
        public const string A_PORT_NAME = "A";
        public const string B_PORT_NAME = "B";
        public const string TRUE_PORT_NAME = "True";
        public const string FALSE_PORT_NAME = "False";

        [NodePropertyPort(ALGORITHM_PORT_NAME, true, typeof(CompareAlgorithm), CompareAlgorithm.Equals, true)]
        public CompareAlgorithm Algorithm;
        #endregion

        #region Properties
        protected override bool HasDone => false;
        #endregion

        #region Constructors
        public CompareNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();

            CreateOutputFlowPort(TRUE_PORT_NAME);
            CreateOutputFlowPort(FALSE_PORT_NAME);
        }

        protected override void UpdatePorts()
        {
            AddElementPort(A_PORT_NAME, true);
            AddElementPort(B_PORT_NAME, true);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            ElementType = GetPortValue(ELEMENT_TYPE_PORT_NAME, ElementType);
            Algorithm = GetPortValue(ALGORITHM_PORT_NAME, Algorithm);
            var result = false;
            var defaultA = InputPropertyPorts.First(x => x.Name == A_PORT_NAME).Value;
            var defaultB = InputPropertyPorts.First(x => x.Name == B_PORT_NAME).Value;
            switch (ElementType)
            {
                case CollectionType.String:
                    result = CompareStrings((string)defaultA, (string)defaultB);
                    break;
                case CollectionType.Int:
                    result = CompareInts((int)defaultA, (int)defaultB);
                    break;
                case CollectionType.Partition:
                    result = ComparePartitions();
                    break;
                case CollectionType.Bool:
                    result = CompareBools((bool)defaultA, (bool)defaultB);
                    break;
            }
            ExecutionState = ExecutePort(result ? TRUE_PORT_NAME : FALSE_PORT_NAME);
        }

        private bool CompareStrings(string defaultA, string defaultB)
        {
            var a = GetPortValue(A_PORT_NAME, defaultA);
            var b = GetPortValue(B_PORT_NAME, defaultB);
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

        private bool CompareInts(int defaultA, int defaultB)
        {
            var a = GetPortValue(A_PORT_NAME, defaultA);
            var b = GetPortValue(B_PORT_NAME, defaultB);
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
            if (a == null || b == null)
            {
                ExecutionState = ExecutionState.Failed;
                return false;
            }
            var r = string.Compare(a.RasterizedData, b.RasterizedData, StringComparison.Ordinal);
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

        private bool CompareBools(bool defaultA, bool defaultB)
        {
            var a = GetPortValue(A_PORT_NAME, defaultA);
            var b = GetPortValue(B_PORT_NAME, defaultB);
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