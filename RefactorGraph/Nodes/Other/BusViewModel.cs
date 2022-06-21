using NodeGraph.Model;
using NodeGraph.ViewModel;

namespace RefactorGraph.Nodes.Other
{
    [NodeViewModel(ViewType = typeof(BusView))]
    public class BusViewModel : NodeViewModel
    {
        #region Constructors
        public BusViewModel(Node node) : base(node) { }
        #endregion
    }
}