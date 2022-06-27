using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NodeGraph;
using NodeGraph.Model;
using NodeGraph.ViewModel;
using RefactorGraph.Nodes;
using RefactorGraph.Nodes.PartitionOperations;
using SelectionMode = NodeGraph.SelectionMode;

namespace RefactorGraph
{
    public partial class DesignerWindowControl : UserControl
    {
        #region Fields
        public static readonly DependencyProperty FlowChartViewModelProperty =
            DependencyProperty.Register("FlowChartViewModel", typeof(FlowChartViewModel), typeof(DesignerWindowControl), new PropertyMetadata(null));
        private Point _contextMenuLocation;
        #endregion

        #region Properties
        public FlowChartViewModel FlowChartViewModel
        {
            get => (FlowChartViewModel)GetValue(FlowChartViewModelProperty);
            set
            {
                SetValue(FlowChartViewModelProperty, value);
                Utils.FlowChartWindow = this;
            }
        }
        #endregion

        #region Constructors
        public DesignerWindowControl()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }
        #endregion

        #region Methods
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NodeGraphManager.OutputDebugInfo = true;
            NodeGraphManager.SelectionMode = SelectionMode.Include;

            NodeGraphManager.BuildFlowChartContextMenu += NodeGraphManager_BuildFlowChartContextMenu;
            NodeGraphManager.BuildNodeContextMenu += NodeGraphManager_BuildNodeContextMenu;
            NodeGraphManager.BuildFlowPortContextMenu += NodeGraphManager_BuildFlowPortContextMenu;
            NodeGraphManager.BuildPropertyPortContextMenu += NodeGraphManager_BuildPropertyPortContextMenu;
            NodeGraphManager.NodeSelectionChanged += NodeGraphManager_NodeSelectionChanged;
            NodeGraphManager.DragEnter += NodeGraphManager_DragEnter;
            NodeGraphManager.DragLeave += NodeGraphManager_DragLeave;
            NodeGraphManager.DragOver += NodeGraphManager_DragOver;
            NodeGraphManager.Drop += NodeGraphManager_Drop;
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            NodeGraphManager.BuildFlowChartContextMenu -= NodeGraphManager_BuildFlowChartContextMenu;
            NodeGraphManager.BuildNodeContextMenu -= NodeGraphManager_BuildNodeContextMenu;
            NodeGraphManager.BuildFlowPortContextMenu -= NodeGraphManager_BuildFlowPortContextMenu;
            NodeGraphManager.BuildPropertyPortContextMenu -= NodeGraphManager_BuildPropertyPortContextMenu;
            NodeGraphManager.NodeSelectionChanged -= NodeGraphManager_NodeSelectionChanged;
            NodeGraphManager.DragEnter -= NodeGraphManager_DragEnter;
            NodeGraphManager.DragLeave -= NodeGraphManager_DragLeave;
            NodeGraphManager.DragOver -= NodeGraphManager_DragOver;
            NodeGraphManager.Drop -= NodeGraphManager_Drop;
            KeyDown -= MainWindow_KeyDown;
            Utils.FlowChartWindow = null;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Key.S == e.Key)
                {
                    NodeGraphManager.Serialize(@"SerializationTest.xml");
                }
                else if (Key.O == e.Key)
                {
                    var flowChartGuid = FlowChartViewModel.Model.Guid;
                    FlowChartViewModel = null;
                    NodeGraphManager.DestroyFlowChart(flowChartGuid);

                    if (NodeGraphManager.Deserialize(@"SerializationTest.xml"))
                    {
                        var flowChart = NodeGraphManager.FlowCharts.First().Value;
                        FlowChartViewModel = flowChart.ViewModel;
                        FlowChartViewModel.View.ZoomAndPan.StartX = 0.0;
                        FlowChartViewModel.View.ZoomAndPan.StartY = 0.0;
                        FlowChartViewModel.View.ZoomAndPan.Scale = 1.0;
                    }
                    else
                    {
                        var flowChart = NodeGraphManager.CreateFlowChart(false, Guid.NewGuid(), typeof(FlowChart));
                        FlowChartViewModel = flowChart.ViewModel;
                    }
                }
            }
        }

        private bool NodeGraphManager_BuildFlowChartContextMenu(object sender, BuildContextMenuArgs args)
        {
            SetDropDownMenuToBeRightAligned();

            var items = args.ContextMenu.Items;
            _contextMenuLocation = args.ModelSpaceMouseLocation;
            items.Clear();

            var typeGroups = typeof(RefactorNodeBase).FindAllDerivedTypes()
                .Where(x => x.HasAttribute<RefactorNodeAttribute>())
                .GroupBy(x => x.GetAttribute<RefactorNodeAttribute>().nodeGroup)
                .OrderBy(x => x.Key);

            foreach (var typeGroup in typeGroups)
            {
                var menuGroupItem = new MenuItem();
                menuGroupItem.Header = typeGroup.Key;
                items.Add(menuGroupItem);

                foreach (var nodeType in typeGroup.OrderBy(x => x.GetAttribute<RefactorNodeAttribute>().nodeType.ToString()))
                {
                    var menuItem = new MenuItem();
                    menuItem.Header = nodeType.GetAttribute<RefactorNodeAttribute>().nodeType;
                    menuItem.CommandParameter = nodeType;
                    menuItem.Click += FlowChart_ContextMenuItem_Click;
                    menuGroupItem.Items.Add(menuItem);
                }
            }
            return 0 < items.Count;
        }

        private static void SetDropDownMenuToBeRightAligned()
        {
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Action setAlignmentValue = () =>
            {
                if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null)
                {
                    menuDropAlignmentField.SetValue(null, false);
                }
            };

            setAlignmentValue();

            SystemParameters.StaticPropertyChanged += (sender, e) =>
            {
                setAlignmentValue();
            };
        }

        private bool NodeGraphManager_BuildNodeContextMenu(object sender, BuildContextMenuArgs args)
        {
            var items = args.ContextMenu.Items;
            return 0 < items.Count;
        }

        private bool NodeGraphManager_BuildFlowPortContextMenu(object sender, BuildContextMenuArgs args)
        {
            var items = args.ContextMenu.Items;
            return 0 < items.Count;
        }

        private bool NodeGraphManager_BuildPropertyPortContextMenu(object sender, BuildContextMenuArgs args)
        {
            var items = args.ContextMenu.Items;
            return 0 < items.Count;
        }

        protected virtual void FlowChart_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var nodeType = menuItem.CommandParameter as Type;

            var flowChartView = FlowChartViewModel.View;
            var flowChart = flowChartView.ViewModel.Model;
            flowChart.History.BeginTransaction("Creating node");
            {
                NodeGraphManager.CreateNode(
                    false, Guid.NewGuid(), FlowChartViewModel.Model, nodeType,
                    _contextMenuLocation.X, _contextMenuLocation.Y, 0);
            }
            flowChart.History.EndTransaction(false);
        }

        private void NodeGraphManager_NodeSelectionChanged(FlowChart flowChart, ObservableCollection<Guid> nodes, NotifyCollectionChangedEventArgs args) { }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (FlowChartViewModel != null)
            {
                Utils.Save(FlowChartViewModel.Model);
            }
        }

        private void Refactor(object sender, RoutedEventArgs e)
        {
            if (FlowChartViewModel == null)
            {
                return;
            }
            Utils.Refactor(FlowChartViewModel.Model);
        }
        #endregion

        #region Drag & Drop Events
        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Handled =
                e.Data.GetDataPresent("NodeEntry") ||
                e.Data.GetDataPresent("GraphEntry");
            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Handled =
                e.Data.GetDataPresent("NodeEntry") ||
                e.Data.GetDataPresent("GraphEntry");
            base.OnDragOver(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Handled =
                e.Data.GetDataPresent("NodeEntry") ||
                e.Data.GetDataPresent("GraphEntry");
            base.OnDrop(e);
        }

        private void NodeGraphManager_Drop(object sender, NodeGraphDragEventArgs args)
        {
            if (args.DragEventArgs.Data.GetData("NodeEntry") is NodeEntryModel nodeEntry)
            {
                var nodeType = Utils.GetNodeType(nodeEntry.nodeType);
                if (nodeType == null)
                {
                    return;
                }
                CreateNode(nodeType, args.ModelSpaceMouseLocation.X, args.ModelSpaceMouseLocation.Y);
            }
            if (args.DragEventArgs.Data.GetData("GraphEntry") is GraphEntryControl graphEntry)
            {
                var nodeType = typeof(ReferenceNode);
                var node = CreateNode(nodeType, args.ModelSpaceMouseLocation.X, args.ModelSpaceMouseLocation.Y);
                node.Header = graphEntry.GraphName;
            }
        }

        private Node CreateNode(Type nodeType, double x, double y)
        {
            Node node;
            var flowChart = FlowChartViewModel.Model;
            flowChart.History.BeginTransaction("Creating node");
            {
                node = NodeGraphManager.CreateNode(
                    false, Guid.NewGuid(), FlowChartViewModel.Model, nodeType,
                    x, y, 0);
            }
            flowChart.History.EndTransaction(false);
            return node;
        }

        private void NodeGraphManager_DragOver(object sender, NodeGraphDragEventArgs args) { }

        private void NodeGraphManager_DragLeave(object sender, NodeGraphDragEventArgs args) { }

        private void NodeGraphManager_DragEnter(object sender, NodeGraphDragEventArgs args) { }
        #endregion // Drag & Drop Events
    }
}