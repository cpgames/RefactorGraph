using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RefactorGraph
{
    public partial class MainWindowControl : UserControl
    {
        #region Fields
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private UIElement _realDragSource;
        private readonly UIElement _dummyDragSource = new UIElement();
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
            RefreshInternal();
        }

        private void CreateRefactorGraph(object sender, RoutedEventArgs e)
        {
            var entry = new GraphEntryControl();
            StackPatterns.Children.Add(entry);
            entry.CreateNewGraph();
        }

        private void Load(object sender, RoutedEventArgs e) { }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            RefreshInternal();
        }

        private void RefreshInternal()
        {
            foreach (UIElement element in StackPatterns.Children)
            {
                if (element is GraphEntryControl refactorGraphEntry)
                {
                    refactorGraphEntry.Unload();
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

        private void PatternsMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Equals(e.Source, StackPatterns))
            {
                _isDown = true;
                _startPoint = e.GetPosition(StackPatterns);
            }
        }

        private void PatternsMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDown = false;
            _isDragging = false;
            _realDragSource?.ReleaseMouseCapture();
        }

        private void PatternsMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                if (_isDragging == false && (Math.Abs(e.GetPosition(StackPatterns).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(e.GetPosition(StackPatterns).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;
                    _realDragSource = e.Source as UIElement;
                    _realDragSource.CaptureMouse();
                    DragDrop.DoDragDrop(_dummyDragSource, new DataObject("UIElement", e.Source, true), DragDropEffects.Move);
                }
            }
        }

        private void PatternsDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void PatternsDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData("UIElement");
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

                _isDown = false;
                _isDragging = false;
                _realDragSource.ReleaseMouseCapture();
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent("UIElement");
            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent("UIElement");
            base.OnDragOver(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent("UIElement");
            base.OnDrop(e);
        }
        #endregion
    }
}