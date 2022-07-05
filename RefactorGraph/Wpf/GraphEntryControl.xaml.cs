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
        private bool _opened;
        private bool _enabled = true;
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
        
        private void UpdateBorder()
        {
            if (Utils.FlowChartWindow != null && Utils.FlowChartWindow.FlowChartViewModel == _flowChartViewModel)
            {
                Border.BorderBrush = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                Border.BorderBrush = new SolidColorBrush(Colors.White);
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                HeaderLabel.Content = value ?
                    "RefactorClick Graph Name:" :
                    "[Disabled] RefactorClick Graph Name:";
                _enabled = value;
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

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Utils.flowChartChanged -= UpdateBorder;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.flowChartChanged += UpdateBorder;
            UpdateBorder();
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
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
                400, 200, 0);
            var connector = NodeGraphManager.CreateConnector(
                false, Guid.NewGuid(), _flowChartViewModel.Model);
            NodeGraphManager.ConnectTo(startNode.OutputFlowPorts[0], connector);
            NodeGraphManager.ConnectTo(getDocumentNode.InputFlowPorts[0], connector);
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

        private void Save(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Edit(object sender, RoutedEventArgs e)
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

        private void Toggle(object sender, RoutedEventArgs e)
        {
            Enabled = !Enabled;
        }
        #endregion
    }
}