using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using NodeGraph;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace RefactorGraph
{
    public partial class MainWindowControl : UserControl
    {
        #region Fields
        private static bool _loaded = false;
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private UIElement _realDragSource;
        #endregion

        #region Constructors
        public MainWindowControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        #endregion

        #region Methods
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                RefreshInternal();
                _loaded = true;
            }
        }

        private void CreateRefactorGraph(object sender, RoutedEventArgs e)
        {
            var entry = new GraphEntryControl();
            StackPatterns.Children.Add(entry);
            entry.CreateNewGraph();
        }
        
        private void Refresh(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Refreshing will delete any unsaved graphs. Are you sure?", "Refresh", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                DesignerWindow.HideAsync().Wait();
                RefreshInternal();
            }
        }

        private void RefreshInternal()
        {
            foreach (UIElement element in StackPatterns.Children)
            {
                if (element is GraphEntryControl refactorGraphEntry && refactorGraphEntry.FlowChartViewModel != null)
                {
                    NodeGraphManager.DestroyFlowChart(refactorGraphEntry.FlowChartViewModel.Model.Guid);
                }
            }

            StackPatterns.Children.Clear();
            var files = Utils.GetGraphFiles();
            foreach (var fileName in files)
            {
                var entryExists = false;
                foreach (UIElement element in StackPatterns.Children)
                {
                    if (element is GraphEntryControl refactorGraphEntry)
                    {
                        if (refactorGraphEntry.GraphName == fileName)
                        {
                            entryExists = true;
                            break;
                        }
                    }
                }
                if (!entryExists)
                {
                    var entry = new GraphEntryControl();
                    StackPatterns.Children.Add(entry);
                    entry.SetFile(fileName);
                }
            }
            if (Utils.refreshAction != null)
            {
                Utils.refreshAction();
            }
        }

        private void ShowToolbar(object sender, RoutedEventArgs e)
        {
            ToolbarWindow.ShowAsync().Wait();
        }

        private void RefactorAll(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in StackPatterns.Children)
            {
                if (element is GraphEntryControl refactorGraphEntry && refactorGraphEntry.Enabled)
                {
                    refactorGraphEntry.Refactor();
                }
            }
        }

        private void EntryMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Equals(e.Source, StackPatterns))
            {
                _isDown = true;
                _startPoint = e.GetPosition(StackPatterns);
            }
        }

        private void EntryMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDown = false;
            _isDragging = false;
            _realDragSource?.ReleaseMouseCapture();
        }

        private void EntryMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                if (_isDragging == false &&
                    (Math.Abs(e.GetPosition(StackPatterns).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(e.GetPosition(StackPatterns).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    if (e.OriginalSource is TextBox)
                    {
                        return;
                    }
                    _isDragging = true;
                    _realDragSource = e.Source as UIElement;
                    _realDragSource.CaptureMouse();
                    DragDrop.DoDragDrop(_realDragSource, new DataObject("GraphEntry", e.Source, true), DragDropEffects.Move);
                }
            }
        }

        private void EntryDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("GraphEntry"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void EntryDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData("GraphEntry");
            if (data != null)
            {
                var y = e.GetPosition(StackPatterns).Y;
                var start = 0.0;
                var index = 0;
                foreach (UIElement element in StackPatterns.Children)
                {
                    start += element.RenderSize.Height;
                    if (start > y)
                    {
                        break;
                    }
                    index++;
                }
                StackPatterns.Children.Remove(_realDragSource);
                StackPatterns.Children.Insert(index, _realDragSource);
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

        private void Help(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/cpgames/RefactorGraph/wiki");
        }
        #endregion
    }
}