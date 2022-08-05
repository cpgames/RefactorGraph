using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RefactorGraph.Nodes;

namespace RefactorGraph
{
    public partial class ToolbarWindowControl : UserControl
    {
        #region Constructors
        public ToolbarWindowControl()
        {
            InitializeComponent();
            PopulateNodes();
            Utils.refreshed += PopulateNodes;
            Unloaded += OnUnloaded;
        }
        #endregion

        #region Methods
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Utils.refreshed -= PopulateNodes;
        }

        private void PopulateNodes()
        {
            Nodes.Children.Clear();
            var nodes = Utils.GetNodeEntries();
            foreach (var nodeGroup in nodes)
            {
                var group = nodeGroup.Key;
                var groupTitle = new Label
                {
                    Content = group,
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = NodeColors.brushes[group],
                    Width = double.NaN
                };

                Nodes.Children.Add(groupTitle);
                var stackPanel = new StackPanel();
                foreach (var nodeEntry in nodeGroup.Value.OrderBy(x => x.nodeName))
                {
                    var nodeEntryControl = new ToolbarNodeEntryControl
                    {
                        NodeEntry = nodeEntry
                    };
                    stackPanel.Children.Add(nodeEntryControl);
                }
                Nodes.Children.Add(stackPanel);
            }
        }
        #endregion
    }
}