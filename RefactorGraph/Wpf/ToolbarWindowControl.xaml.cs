using System.Windows;
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
                    var nodeEntryControl = new ToolbarNodeEntryControl
                    {
                        NodeEntry = nodeEntry
                    };
                    stackPanel.Children.Add(nodeEntryControl);
                }
                Nodes.Children.Add(expander);
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent("GraphEntry");
            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent("GraphEntry");
            base.OnDragOver(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent("GraphEntry");
            base.OnDrop(e);
        }

        private void EntryDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData("GraphEntry") is GraphEntryControl graphEntry)
            {
                graphEntry.FlowChartViewModel.Model.IsReference = true;
                graphEntry.Save();
                PopulateNodes();
            }
        }
        #endregion
    }
}