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
            Utils.refreshAction += PopulateNodes;
            Unloaded += OnUnloaded;
        }
        #endregion

        #region Methods
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Utils.refreshAction -= PopulateNodes;
        }

        private void PopulateNodes()
        {
            Nodes.Children.Clear();
            var nodes = Utils.GetNodeEntries();
            foreach (var nodeGroup in nodes)
            {
                var group = nodeGroup.Key;
                var expander = new Expander
                {
                    Header = group,
                    Foreground = new SolidColorBrush(Colors.White),
                    IsExpanded = true
                };
                var stackPanel = new StackPanel();
                expander.Content = stackPanel;
                foreach (var nodeEntry in nodeGroup.Value)
                {
                    if (nodeEntry.nodeType == RefactorNodeType.Reference)
                    {
                        continue;
                    }
                    var nodeEntryControl = new ToolbarNodeEntryControl
                    {
                        NodeEntry = nodeEntry
                    };
                    stackPanel.Children.Add(nodeEntryControl);
                }
                Nodes.Children.Add(expander);
            }
        }
        #endregion
    }
}