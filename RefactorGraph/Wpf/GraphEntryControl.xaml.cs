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
using RefactorGraphdCore.Data;
using MessageBox = System.Windows.Forms.MessageBox;
using Panel = System.Windows.Controls.Panel;
using UserControl = System.Windows.Controls.UserControl;

namespace RefactorGraph
{
    public partial class GraphEntryControl : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private FlowChartViewModel _flowChartViewModel;
        private Chunk _document = new Chunk
        {
            content = "This is a test"
        };
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
                    foreach (var file in Utils.GetGraphFiles())
                    {
                        if (file == _graphName)
                        {
                            if (!Utils.RenameFile(_graphName, value))
                            {
                                return;
                            }
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
        #endregion

        #region Constructors
        public GraphEntryControl()
        {
            InitializeComponent();
            DataContext = this;

            RefactorNodeBase.NodeCreatedEvent += RefactorNodeBase_NodeCreatedEvent;
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
            RefactorNodeBase.NodeCreatedEvent -= RefactorNodeBase_NodeCreatedEvent;
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

        private void RefactorNodeBase_NodeCreatedEvent(object sender, EventArgs e)
        {
            if (sender is GetDocumentNode getDocumentNode)
            {
                getDocumentNode.GetDocumentCallback = GetDocument;
            }
            else if (sender is SetDocumentNode setDocumentNode)
            {
                setDocumentNode.SetDocumentCallback = SetDocument;
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

        private Chunk GetDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var document = GetActiveDocument();
            if (document == null)
            {
                return null;
            }
            var chunk = new Chunk();
            if (!document.Selection.IsEmpty)
            {
                chunk.content = document.Selection.Text;
            }
            else
            {
                var editPoint = document.StartPoint.CreateEditPoint();
                var docText = editPoint.GetText(document.EndPoint);
                chunk.content = docText;
            }
            return chunk;
        }

        private void SetDocument(Chunk chunk)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var document = GetActiveDocument();
            if (document.Selection.IsEmpty)
            {
                document.Selection.SelectAll();
            }
            document.Selection.Insert(chunk.content);
        }

        public void CreateNewGraph()
        {
            var flowChart = NodeGraphManager.CreateFlowChart(false, Guid.NewGuid(), typeof(FlowChart));
            _flowChartViewModel = flowChart.ViewModel;

            var pattern = "NewRefactorGraph";
            var index = 0;
            var files = Utils.GetGraphFiles().ToList();
            string graphName;
            do
            {
                graphName = $"{pattern}{index}";
                index++;
            } while (files.Any(x => x == graphName));
            GraphName = graphName;
            BuildDefaultGraph();
            Save();
            OpenGraphWindowAsync().Wait();
        }

        private void BuildDefaultGraph()
        {
            var getDocumentNode = NodeGraphManager.CreateNode(
                false, Guid.NewGuid(), _flowChartViewModel.Model, typeof(GetDocumentNode),
                200, 200, 0);

            var setDocumentNode = NodeGraphManager.CreateNode(
                false, Guid.NewGuid(), _flowChartViewModel.Model, typeof(SetDocumentNode),
                500, 200, 0);

            var connector1 = NodeGraphManager.CreateConnector(
                false, Guid.NewGuid(), _flowChartViewModel.Model);
            NodeGraphManager.ConnectTo(getDocumentNode.OutputFlowPorts[0], connector1);
            NodeGraphManager.ConnectTo(setDocumentNode.InputFlowPorts[0], connector1);

            var connector2 = NodeGraphManager.CreateConnector(
                false, Guid.NewGuid(), _flowChartViewModel.Model);
            NodeGraphManager.ConnectTo(getDocumentNode.OutputPropertyPorts[0], connector2);
            NodeGraphManager.ConnectTo(setDocumentNode.InputPropertyPorts[0], connector2);
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
            NodeGraphManager.ClearScreenLogs(_flowChartViewModel.Model);
            var getDocumentNodes = NodeGraphManager.FindNode(_flowChartViewModel.Model, "Get Document");
            if (getDocumentNodes.Count == 0)
            {
                NodeGraphManager.AddScreenLog(_flowChartViewModel.Model, "You need to place a 'Get Document' node.");
                return;
            }
            var setDocumentNodes = NodeGraphManager.FindNode(_flowChartViewModel.Model, "Set Document");
            if (setDocumentNodes.Count == 0)
            {
                NodeGraphManager.AddScreenLog(_flowChartViewModel.Model, "You need to place a 'Set Document' node.");
                return;
            }

            foreach (var node in getDocumentNodes.OfType<GetDocumentNode>())
            {
                node.OnPreExecute(null);
                node.OnExecute(null);
                node.OnPostExecute(null);
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

        public void Save(string dir = null)
        {
            Utils.Save(GraphName, _flowChartViewModel.Model, dir);
        }

        private void Toggle(object sender, RoutedEventArgs e)
        {
            Enabled = !Enabled;
        }
        #endregion
    }
}