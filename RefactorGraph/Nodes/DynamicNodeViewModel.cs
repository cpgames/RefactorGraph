using NodeGraph.Model;
using NodeGraph.ViewModel;

namespace RefactorGraph.Nodes
{
    [NodeViewModel(ViewType = typeof(DynamicNodeView))]
    public class DynamicNodeViewModel : NodeViewModel
    {
        #region Constructors
        public DynamicNodeViewModel(Node node) : base(node) { }
        #endregion
    }
}