using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NodeGraph;
using NodeGraph.Model;
using NodeGraph.ViewModel;
using SelectionMode = NodeGraph.SelectionMode;

namespace RefactorGraph
{
    public partial class DesignerWindowControl : UserControl
    {
        #region Fields
        public static readonly DependencyProperty FlowChartViewModelProperty =
            DependencyProperty.Register("FlowChartViewModel", typeof(FlowChartViewModel), typeof(DesignerWindowControl), new PropertyMetadata(null));
        private Point _contextMenuLocation;
        private readonly Type[] _nodeTypes =
        {
            typeof(GetDocumentNode),
            typeof(SetDocumentNode),
            typeof(PatternNode),
            typeof(SplitRegexNode),
            typeof(SplitIndexNode),
            typeof(ReplaceNode),
            typeof(JoinNode),
            typeof(MergeNode),
            typeof(ForEachNode),
            typeof(ChunkNode),
            typeof(ChunkCollectionNode),
            typeof(OrderByChunkIndexNode),
            typeof(GetElementNode),
            typeof(GetCollectionSizeNode),
            typeof(EqualsNode),
            typeof(IntNode),
            typeof(AddNode),
            typeof(IntToChunkNode),
            typeof(SetNode),
            typeof(TwoBusNode),
            typeof(ThreeBusNode),
            typeof(FilterNode),
            typeof(ClearNode),
            typeof(OrderAlphabeticalNode)
        };
        #endregion

        #region Properties
        public FlowChartViewModel FlowChartViewModel
        {
            get => (FlowChartViewModel)GetValue(FlowChartViewModelProperty);
            set => SetValue(FlowChartViewModelProperty, value);
        }
        #endregion

        #region Constructors
        public DesignerWindowControl()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
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
            var items = args.ContextMenu.Items;

            _contextMenuLocation = args.ModelSpaceMouseLocation;

            items.Clear();

            foreach (var nodeType in _nodeTypes)
            {
                var menuItem = new MenuItem();

                var NodeAttrs = nodeType.GetCustomAttributes(typeof(NodeAttribute), false) as NodeAttribute[];
                if (1 != NodeAttrs.Length)
                {
                    throw new ArgumentException(string.Format("{0} must have NodeAttribute", nodeType.Name));
                }

                menuItem.Header = "Create " + nodeType.Name;
                menuItem.CommandParameter = nodeType;
                menuItem.Click += FlowChart_ContextMenuItem_Click;
                items.Add(menuItem);
            }

            return 0 < items.Count;
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

        #region Selection Events
        private void NodeGraphManager_NodeSelectionChanged(FlowChart flowChart, ObservableCollection<Guid> nodes, NotifyCollectionChangedEventArgs args) { }
        #endregion // Selection Events
        #endregion

        #region Drag & Drop Events
        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Handled = e.Data.GetData(typeof(RefactorNodeType)) != null;
            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Handled = e.Data.GetData(typeof(RefactorNodeType)) != null;
            base.OnDragOver(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Handled = e.Data.GetData(typeof(RefactorNodeType)) != null;
            base.OnDrop(e);
        }

        private void NodeGraphManager_Drop(object sender, NodeGraphDragEventArgs args)
        {
            var flowChartView = FlowChartViewModel.View;

            var eType = (RefactorNodeType)args.DragEventArgs.Data.GetData(typeof(RefactorNodeType));
            var nodeType = _nodeTypes[(int)eType];

            var flowChart = flowChartView.ViewModel.Model;
            flowChart.History.BeginTransaction("Creating node");
            {
                NodeGraphManager.CreateNode(
                    false, Guid.NewGuid(), FlowChartViewModel.Model, nodeType,
                    args.ModelSpaceMouseLocation.X, args.ModelSpaceMouseLocation.Y, 0);
            }
            flowChart.History.EndTransaction(false);
        }

        private void NodeGraphManager_DragOver(object sender, NodeGraphDragEventArgs args) { }

        private void NodeGraphManager_DragLeave(object sender, NodeGraphDragEventArgs args) { }

        private void NodeGraphManager_DragEnter(object sender, NodeGraphDragEventArgs args) { }
        #endregion // Drag & Drop Events
    }
}