using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using NodeGraph;
using NodeGraph.Model;
using NodeGraph.ViewModel;
using RefactorGraph.Nodes.Other;
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
        #endregion

        #region Properties
        public string GraphName
        {
            get => _graphName;
            set
            {
                if (_graphName != value)
                {
                    if (NodeGraphManager.FlowCharts.Values.Any(x => x.Name == value))
                    {
                        MessageBox.Show($"Graph <{value}> already exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    foreach (var file in Utils.GetGraphFiles())
                    {
                        if (file == _graphName)
                        {
                            if (!Utils.RenameFile(_graphName, value))
                            {
                                return;
                            }
                            _flowChartViewModel.Model.Name = value;
                            Utils.Save(_flowChartViewModel.Model);
                        }
                    }
                    _graphName = value;
                    RaisePropertyChanged("GraphName");
                }
            }
        }

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
        private void UpdateEditing()
        {
            if (Utils.FlowChartWindow != null && Utils.FlowChartWindow.FlowChartViewModel == _flowChartViewModel)
            {
                Border.BorderBrush = new SolidColorBrush(Colors.Orange);
                Details.Visibility = Visibility.Visible;
            }
            else
            {
                Border.BorderBrush = new SolidColorBrush(Colors.White);
                Details.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Utils.flowChartChanged -= UpdateEditing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.flowChartChanged += UpdateEditing;
            UpdateEditing();
        }

        private void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetFile(string fileName)
        {
            try
            {
                Utils.Load(fileName, out var flowChart);
                _flowChartViewModel = flowChart.ViewModel;
                _graphName = fileName;
                RaisePropertyChanged("GraphName");
            }
            catch (Exception e)
            {
                var result = MessageBox.Show($"{e.Message}\n\nDelete graph?", $"Failed to load {fileName}", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    Utils.Delete(fileName);
                }
                ((Panel)Parent).Children.Remove(this);
            }
        }

        public void CreateNewGraph()
        {
            var flowChart = NodeGraphManager.CreateFlowChart(false, Guid.NewGuid(), typeof(FlowChart));

            _flowChartViewModel = flowChart.ViewModel;
            var name = "NewRefactorGraph";
            var index = 0;
            var files = Utils.GetGraphFiles().ToList();
            string graphName;
            do
            {
                graphName = $"{name}{index}";
                index++;
            } while (files.Any(x => x == graphName));
            GraphName = graphName;
            flowChart.Name = GraphName;
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
            if (result == DialogResult.Yes && Utils.Delete(GraphName))
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
            Utils.Save(_flowChartViewModel.Model);
        }
        #endregion
    }
}