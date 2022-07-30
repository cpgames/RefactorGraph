using System;
using System.Linq;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.GetReferenceArg)]
    public class GetReferenceArgNode : TypedRefactorNodeBase
    {
        #region Fields
        public const string VALUE_PORT_NAME = "Value";

        public object value;
        #endregion

        #region Constructors
        public GetReferenceArgNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void UpdatePorts()
        {
            AddElementPort(VALUE_PORT_NAME, false, -1, false);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            if (value == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            switch (ElementType)
            {
                case CollectionType.String:
                    if (!(value is string))
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                    break;
                case CollectionType.Int:
                    if (!(value is int))
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                    break;
                case CollectionType.Partition:
                    if (!(value is Partition))
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                    break;
                case CollectionType.Bool:
                    if (!(value is bool))
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                    break;
            }
            OutputPropertyPorts.First(x => x.Name == VALUE_PORT_NAME).Value = value;
        }
        #endregion
    }
}