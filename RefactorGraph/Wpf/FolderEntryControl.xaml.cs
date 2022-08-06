using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace RefactorGraph.Wpf
{
    public partial class FolderEntryControl : UserControl
    {
        #region Fields
        private string _folder;
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private UIElement _realDragSource;
        #endregion

        #region Constructors
        public FolderEntryControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void Remove()
        {
            while (GraphEntries.Children.OfType<GraphEntryControl>().Any())
            {
                GraphEntries.Children.OfType<GraphEntryControl>().First().Remove();
            }
            ((Panel)Parent).Children.Remove(this);
        }

        public void SaveAll()
        {
            foreach (var graphEntry in GraphEntries.Children.OfType<GraphEntryControl>())
            {
                graphEntry.Save();
            }
        }

        public void SetFolder(string folder)
        {
            _folder = folder;
            FolderName.Content = Path.GetFileName(folder);
            Refresh();
        }

        private void Refresh()
        {
            while (GraphEntries.Children.OfType<GraphEntryControl>().Any())
            {
                GraphEntries.Children.OfType<GraphEntryControl>().First().Remove();
            }
            var filePaths = Utils.GetGraphFiles(_folder);
            foreach (var filePath in filePaths)
            {
                var entry = new GraphEntryControl();
                GraphEntries.Children.Add(entry);
                entry.SetFile(filePath);
            }
            Utils.refreshed?.Invoke();
        }

        private void ExpandClicked(object sender, RoutedEventArgs e)
        {
            GraphEntries.Visibility = Visibility.Visible;
            ExpandButton.Visibility = Visibility.Collapsed;
            CollapseButton.Visibility = Visibility.Visible;
        }

        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            GraphEntries.Visibility = Visibility.Collapsed;
            ExpandButton.Visibility = Visibility.Visible;
            CollapseButton.Visibility = Visibility.Collapsed;
        }

        private void NewClicked(object sender, RoutedEventArgs e)
        {
            var entry = new GraphEntryControl();
            GraphEntries.Children.Add(entry);
            entry.CreateNewGraph(_folder);
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Refreshing will delete all unsaved changes to graphs in this folder.\nSave all changes?",
                $"Refresh {FolderName.Content}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                SaveAll();
                Refresh();
            }
            else if (result == DialogResult.No)
            {
                Refresh();
            }
        }

        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Removing the folder will delete all unsaved changes to graphs in this folder (graphs will not be deleted).\nSave all changes?",
                $"Remove {FolderName.Content}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                SaveAll();
                Remove();
                Utils.RemoveIncludeFolder(_folder);
            }
            else if (result == DialogResult.No)
            {
                Remove();
                Utils.RemoveIncludeFolder(_folder);
            }
        }

        private void GraphEntryMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Equals(e.Source, GraphEntries) &&
                !(e.OriginalSource is TextBox))
            {
                _isDown = true;
                _startPoint = e.GetPosition(GraphEntries);
            }
        }

        private void GraphEntryMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDown = false;
            _isDragging = false;
            _realDragSource?.ReleaseMouseCapture();
        }

        private void GraphEntryMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown && !_isDragging)
            {
                if (Math.Abs(e.GetPosition(GraphEntries).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(e.GetPosition(GraphEntries).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    _realDragSource = e.Source as UIElement;
                    _realDragSource.CaptureMouse();
                    DragDrop.DoDragDrop(_realDragSource, new DataObject("GraphEntry", e.Source, true), DragDropEffects.Move);
                }
            }
        }

        private void GraphEntryDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("GraphEntry"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void OpenClicked(object sender, RoutedEventArgs e)
        {
            Utils.OpenInExplorer(_folder);
        }
        #endregion
    }
}