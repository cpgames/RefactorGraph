using NodeGraph.Model;
using NodeGraph.ViewModel;

namespace RefactorGraph
{
    [NodePropertyPortViewModel(ViewType = typeof(PatternPropertyPortView))]
    public class PatternPropertyPortViewModel : NodePropertyPortViewModel
    {
        public PatternPropertyPortViewModel(NodePropertyPort nodeProperty) : base(nodeProperty) { }
    }
}