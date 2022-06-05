using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace RefactorGraph
{
    public partial class ToolbarWindowControl : UserControl
    {
        #region Constructors
        public ToolbarWindowControl()
        {
            InitializeComponent();
            PopulateNodes();
        }
        #endregion

        #region Methods
        private void PopulateNodes()
        {
            var nodeTypeGroups = typeof(RefactorNodeBase).FindAllDerivedTypes()
                .Where(x => x.HasAttribute<RefactorNodeAttribute>())
                .GroupBy(x => x.GetAttribute<RefactorNodeAttribute>().group);

            foreach (var nodeTypeGroup in nodeTypeGroups.OrderBy(x => x.Key))
            {
                var group = nodeTypeGroup.Key;
                var expander = new Expander();
                expander.Header = group;
                expander.Foreground = new SolidColorBrush(Colors.White);
                var stackPanel = new StackPanel();
                expander.Content = stackPanel;
                foreach (var nodeType in nodeTypeGroup)
                {
                    var nodeEntry = new NodeEntryControl();
                    nodeEntry.NodeType = nodeType.GetAttribute<RefactorNodeAttribute>().nodeType;
                    stackPanel.Children.Add(nodeEntry);
                }
                Nodes.Children.Add(expander);
            }
        }
        #endregion
    }
}