using System.Windows;
using System.Windows.Controls;

namespace RefactorGraph
{
    public partial class MainWindowControl : UserControl
    {
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
        #endregion
    }
}