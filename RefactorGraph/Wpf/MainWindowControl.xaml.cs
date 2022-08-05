using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using RefactorGraph.Wpf;
using MessageBox = System.Windows.Forms.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace RefactorGraph
{
    public partial class MainWindowControl : UserControl
    {
        #region Fields
        private static bool _loaded;
        #endregion

        #region Constructors
        public MainWindowControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Utils.AddIncludeFolder("RefactorGraphs");
        }
        #endregion

        #region Methods
        public void SaveAll()
        {
            foreach (var folderEntry in FolderEntries.Children.OfType<FolderEntryControl>())
            {
                folderEntry.SaveAll();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                Refresh();
                _loaded = true;
            }
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Refreshing will delete all unsaved graphs.\nSave all changes?",
                "Refresh Everything", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
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

        private void Refresh()
        {
            while (FolderEntries.Children.OfType<FolderEntryControl>().Any())
            {
                FolderEntries.Children.OfType<FolderEntryControl>().First().Remove();
            }
            var folders = Utils.GetIncludedFolderPaths();
            foreach (var folder in folders)
            {
                var entry = new FolderEntryControl();
                FolderEntries.Children.Add(entry);
                entry.SetFolder(folder);
            }
            Utils.refreshed?.Invoke();
        }

        private void ShowToolbarClicked(object sender, RoutedEventArgs e)
        {
            ToolbarWindow.ShowAsync().Wait();
        }

        private void HelpClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/cpgames/RefactorGraph/wiki");
        }

        private void AddFolderClicked(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var folderPath = folderBrowserDialog.SelectedPath;
                if (!Utils.GetGraphFiles(folderPath).Any())
                {
                    var resultNoFiles = MessageBox.Show($"No graph files found in {folderPath}\n\nAdd folder anyway?", "Add folder",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (resultNoFiles == DialogResult.No)
                    {
                        return;
                    }
                }
                if (Utils.AddIncludeFolder(folderPath))
                {
                    var folderEntry = new FolderEntryControl();
                    FolderEntries.Children.Add(folderEntry);
                    folderEntry.SetFolder(folderPath);
                }
            }
        }
        #endregion
    }
}