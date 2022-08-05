using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using NodeGraph;
using NodeGraph.Model;
using NodeGraph.ViewModel;
using RefactorGraph.Nodes.Other;
using RefactorGraph.Nodes.PartitionOperations;
using MessageBox = System.Windows.Forms.MessageBox;
using Panel = System.Windows.Controls.Panel;
using UserControl = System.Windows.Controls.UserControl;

namespace RefactorGraph
{
    public partial class GraphEntryControl : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private FlowChartViewModel _flowChartViewModel;
        private string _graphName = "NewRefactorGraph";
        private string _folderPath;
        #endregion

        #region Properties
        public string GraphName
        {
            get => _graphName;
            set
            {
                if (_graphName != value && !string.IsNullOrEmpty(value))
                {
                    if (!Utils.RenameFile(GraphGuid, value))
                    {
                        return;
                    }
                    _flowChartViewModel.Model.Name = value;
                    _graphName = value;
                    RaisePropertyChanged("GraphName");
                }
            }
        }

        public Guid GraphGuid => _flowChartViewModel.Model.Guid;

        public FlowChartViewModel FlowChartViewModel => _flowChartViewModel;
        #endregion

        #region Constructors
        public GraphEntryControl()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Window_Loaded;
            Unloaded += Window_Unloaded;
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Utils.flowChartChanged -= UpdateEditing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.flowChartChanged += UpdateEditing;
            UpdateEditing();
        }

        private void UpdateEditing()
        {
            if (Utils.FlowChartWindow != null && Utils.FlowChartWindow.FlowChartViewModel == _flowChartViewModel)
            {
                EntryBackground.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
            }
            else
            {
                EntryBackground.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e"));
            }
        }

        public void Remove()
        {
            Utils.RemoveFlowChart(GraphGuid);
            foreach (var referenceNode in FlowChartViewModel.Model.Nodes.OfType<ReferenceNode>())
            {
                if (referenceNode.referencedFlowChart != null)
                {
                    Utils.RemoveFlowChart(referenceNode.referencedFlowChart.Guid);
                }
            }
            ((Panel)Parent).Children.Remove(this);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetFile(string filePath)
        {
            try
            {
                Utils.Load(filePath, out var flowChart);
                _flowChartViewModel = flowChart.ViewModel;
                _graphName = Path.GetFileNameWithoutExtension(filePath);
                _folderPath = Path.GetDirectoryName(filePath);
                RaisePropertyChanged("GraphName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, $"Failed to load {filePath}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ((Panel)Parent).Children.Remove(this);
            }
        }

        public void CreateNewGraph(string folder)
        {
            _folderPath = folder;
            var flowChart = NodeGraphManager.CreateFlowChart(false, Guid.NewGuid(), typeof(FlowChart));

            _flowChartViewModel = flowChart.ViewModel;
            var name = "NewRefactorGraph";
            var index = 0;
            var files = Utils.GetGraphFiles(folder).ToList();
            string graphName;
            do
            {
                graphName = $"{name}{index}";
                index++;
            } while (files.Any(x => x == graphName));
            _graphName = graphName;
            flowChart.Name = _graphName;
            RaisePropertyChanged("GraphName");
            BuildDefaultGraph();
            Save();
            OpenGraphWindowAsync().Wait();
        }

        private void BuildDefaultGraph()
        {
            var startNode = NodeGraphManager.CreateNode(
                false, Guid.NewGuid(), _flowChartViewModel.Model, typeof(StartNode),
                200, 200, 0);
            var getDocumentNode = NodeGraphManager.CreateNode(
                false, Guid.NewGuid(), _flowChartViewModel.Model, typeof(GetCurrentDocumentNode),
                350, 200, 0);
            var connector = NodeGraphManager.CreateConnector(
                false, Guid.NewGuid(), _flowChartViewModel.Model);
            NodeGraphManager.ConnectTo(startNode.OutputFlowPorts[0], connector);
            NodeGraphManager.ConnectTo(getDocumentNode.InputFlowPorts[0], connector);
            var getDocumentPartitionNode = NodeGraphManager.CreateNode(
                false, Guid.NewGuid(), _flowChartViewModel.Model, typeof(GetDocumentPartitionNode),
                600, 200, 0);
            var connector2 = NodeGraphManager.CreateConnector(
                false, Guid.NewGuid(), _flowChartViewModel.Model);
            NodeGraphManager.ConnectTo(getDocumentNode.OutputFlowPorts[0], connector2);
            NodeGraphManager.ConnectTo(getDocumentPartitionNode.InputFlowPorts[0], connector2);
            var connector3 = NodeGraphManager.CreateConnector(
                false, Guid.NewGuid(), _flowChartViewModel.Model);
            NodeGraphManager.ConnectTo(getDocumentNode.OutputPropertyPorts[0], connector3);
            NodeGraphManager.ConnectTo(getDocumentPartitionNode.InputPropertyPorts[0], connector3);
        }

        private void Delete(object sender, RoutedEventArgs routedEventArgs)
        {
            var result = MessageBox.Show($"Delete {GraphName}?", "Delete RefactorClick Graph", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes && Utils.Delete(GraphGuid))
            {
                if (Utils.FlowChartWindow != null && Utils.FlowChartWindow.FlowChartViewModel == _flowChartViewModel)
                {
                    DesignerWindow.HideAsync().Wait();
                }
                ((Panel)Parent).Children.Remove(this);
            }
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void EditClicked(object sender, RoutedEventArgs e)
        {
            OpenGraphWindowAsync().Wait();
        }

        public void Refactor()
        {
            if (_flowChartViewModel == null)
            {
                return;
            }
            Utils.Refactor(_flowChartViewModel.Model);
        }

        private void RefactorClick(object sender, RoutedEventArgs e)
        {
            Refactor();
        }

        private async Task OpenGraphWindowAsync()
        {
            var task = DesignerWindow.ShowAsync();
            var window = await task;
            if (window != null)
            {
                var flowChartWindow = (DesignerWindowControl)window.Content;
                flowChartWindow.FlowChartViewModel = _flowChartViewModel;
            }
        }

        public void Save()
        {
            var filePath = Path.Combine(_folderPath, GraphName + ".rgraph");
            Utils.Save(_flowChartViewModel.Model, filePath);
        }

        private void ExpandClicked(object sender, RoutedEventArgs e)
        {
            Details.Visibility = Visibility.Visible;
            ExpandButton.Visibility = Visibility.Collapsed;
            CollapseButton.Visibility = Visibility.Visible;
            BtnRun.Visibility = Visibility.Collapsed;
        }

        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            Details.Visibility = Visibility.Collapsed;
            ExpandButton.Visibility = Visibility.Visible;
            CollapseButton.Visibility = Visibility.Collapsed;
            BtnRun.Visibility = Visibility.Visible;
        }
        #endregion
    }
}