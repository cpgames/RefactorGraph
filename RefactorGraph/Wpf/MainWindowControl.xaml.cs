using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using GiveFeedbackEventArgs = System.Windows.GiveFeedbackEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Forms.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace RefactorGraph
{
    public partial class MainWindowControl : UserControl
    {
        #region Fields
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private UIElement _realDragSource;
        private Window _dragDropWindow;
        #endregion

        #region Constructors
        public MainWindowControl()
        {
            //var dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
                    if (e.OriginalSource is System.Windows.Controls.TextBox)
                    {
                        return;
                    }
                    _isDragging = true;
                    _realDragSource = e.Source as UIElement;
                    _realDragSource.CaptureMouse();
                    CreateDragDropCursor();
                    DragDrop.DoDragDrop(_realDragSource, new DataObject("GraphEntry", e.Source, true), DragDropEffects.Move);
                }
            }
        }

        private void EntryDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("GraphEntry"))
            {
                e.Effects = DragDropEffects.Move;
                CreateDragDropCursor();
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

        private void EntryGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (_dragDropWindow != null)
            {
                _dragDropWindow.Left = Utils.actHook.mouseX;
                _dragDropWindow.Top = Utils.actHook.mouseY;
            }
        }

        private void CreateDragDropCursor()
        {
            if (_dragDropWindow != null)
            {
                return;
            }
            _realDragSource.Effect = new DropShadowEffect
            {
                Color = new Color { A = 50, R = 0, G = 0, B = 0 },
                Direction = 320,
                ShadowDepth = 0,
                Opacity = .75
            };
            _realDragSource.Opacity = 0.5;
            CreateDragDropWindow(_realDragSource);

            Utils.actHook.OnMouseActivity += ActHookOnOnMouseActivity;
        }

        private void ActHookOnOnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DestroyDragDropCursor();
            }
        }

        private void DestroyDragDropCursor()
        {
            if (_dragDropWindow != null)
            {
                _dragDropWindow.Close();
                _dragDropWindow = null;
            }
            if (_realDragSource != null)
            {
                _realDragSource.Effect = null;
                _realDragSource.Opacity = 1;
                _realDragSource.ReleaseMouseCapture();
            }
            _isDown = false;
            _isDragging = false;
            Utils.actHook.OnMouseActivity -= ActHookOnOnMouseActivity;
        }

        private void CreateDragDropWindow(Visual dragElement)
        {
            _dragDropWindow = new Window();
            _dragDropWindow.WindowStyle = WindowStyle.None;
            _dragDropWindow.AllowsTransparency = true;
            _dragDropWindow.AllowDrop = false;
            _dragDropWindow.Background = null;
            _dragDropWindow.IsHitTestVisible = false;
            _dragDropWindow.SizeToContent = SizeToContent.WidthAndHeight;
            _dragDropWindow.Topmost = true;
            _dragDropWindow.ShowInTaskbar = false;

            var r = new Rectangle();
            r.Width = ((FrameworkElement)dragElement).ActualWidth;
            r.Height = ((FrameworkElement)dragElement).ActualHeight;
            r.Fill = new VisualBrush(dragElement);
            _dragDropWindow.Content = r;

            _dragDropWindow.Left = Utils.actHook.mouseX;
            _dragDropWindow.Top = Utils.actHook.mouseY;
            _dragDropWindow.MouseUp += DragDropWindowMouseUp;
            _dragDropWindow.Show();
        }

        private void DragDropWindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            DestroyDragDropCursor();
        }
        #endregion
    }
}