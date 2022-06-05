using System;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Property, nodeType = RefactorNodeType.Pattern)]
    public class PatternNode : VariableNode<Pattern>
    {
        #region Properties
        protected override bool HasSetter => false;
        protected override bool HasEditor => true;
        protected override Type ViewModelTypeOverride => typeof(PatternPropertyPortViewModel);
        #endregion

        #region Constructors
        public PatternNode(Guid guid, FlowChart flowChart) : base(guid, flowChart, RefactorNodeType.Pattern) {
        }
        #endregion
    }
}