using NodeGraph.Model;
using NodeGraph.ViewModel;

namespace RefactorGraph
{
    [NodeViewModel(ViewType = typeof(BusView))]
    public class BusViewModel : NodeViewModel
    {
        #region Constructors
        public BusViewModel(Node node) : base(node) { }
        #endregion
    }
}