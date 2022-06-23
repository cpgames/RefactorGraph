using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
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
        private DesignerWindowControl _flowChartWindow;
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

        public bool Opened
        {
            get => _opened;
            set
            {
                if (_opened == value)
                {
                    return;
                }
                _opened = value;
                if (_opened)
                {
                    Border.BorderBrush = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    _flowChartWindow = null;
                    Border.BorderBrush = new SolidColorBrush(Colors.White);
                }
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
            Unloaded += MainWindow_Unloaded;
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
                var result = MessageBox.Show($"{e.Message}\n\n Delete graph?", $"Failed to load {fileName}", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    Utils.Delete(fileName);
                }
                ((Panel)Parent).Children.Remove(this);
            }
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Unload();
        }

        public void Unload()
        {
            if (_flowChartViewModel != null)
            {
                if (_flowChartWindow != null && _flowChartWindow.FlowChartViewModel == _flowChartViewModel)
                {
                    DesignerWindow.HideAsync().Wait();
                    _flowChartWindow = null;
                }
                var flowChartGuid = _flowChartViewModel.Model.Guid;
                NodeGraphManager.DestroyFlowChart(flowChartGuid);
                _flowChartViewModel = null;
            }
        }

        private TextDocument GetActiveDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var doc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument;
            if (doc == null)
            {
                MessageBox.Show("No file opened", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            var textDocument = doc.Object() as TextDocument;
            if (textDocument == null)
            {
                MessageBox.Show("Only text documents can be refactored", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return textDocument;
        }

        private Partition GetDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var document = GetActiveDocument();
            if (document == null)
            {
                return null;
            }
            var chunk = new Partition();
            if (!document.Selection.IsEmpty)
            {
                chunk.Data = document.Selection.Text;
            }
            else
            {
                var editPoint = document.StartPoint.CreateEditPoint();
                chunk.Data = editPoint.GetText(document.EndPoint);
            }
            return chunk;
        }

        private void SetDocument(Partition documentPartition)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var document = GetActiveDocument();
            if (document.Selection.IsEmpty)
            {
                document.Selection.SelectAll();
            }
            document.Selection.Insert(documentPartition.Data);
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
            NodeGraphManager.CreateNode(
                false, Guid.NewGuid(), _flowChartViewModel.Model, typeof(StartNode),
                200, 200, 0);
        }

        private void Delete(object sender, RoutedEventArgs routedEventArgs)
        {
            var result = MessageBox.Show($"Delete {GraphName}?", "Delete RefactorClick Graph", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes && Utils.Delete(GraphName))
            {
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
            NodeGraphManager.ClearScreenLogs(_flowChartViewModel.Model);
            if (!Utils.ValidateGraph(_flowChartViewModel.Model, out var startNode))
            {
                return;
            }
            try
            {
                startNode.Result = GetDocument();
                var originalData = startNode.Result.Data;
                startNode.OnPreExecute(null);
                startNode.OnExecute(null);
                startNode.OnPostExecute(null);
                if (startNode.Success &&
                    startNode.Result.Data != originalData)
                {
                    SetDocument(startNode.Result);
                }
            }
            catch (Exception e)
            {
                NodeGraphManager.AddScreenLog(_flowChartViewModel.Model, e.Message);
            }
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
                if (_flowChartWindow == null)
                {
                    _flowChartWindow = (DesignerWindowControl)window.Content;
                }
                _flowChartWindow.FlowChartViewModel = _flowChartViewModel;
            }

            foreach (var child in ((Panel)Parent).Children)
            {
                if (child is GraphEntryControl refactorGraphEntry)
                {
                    refactorGraphEntry.Opened = false;
                }
            }
            Opened = true;
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