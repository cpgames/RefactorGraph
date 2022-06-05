using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace RefactorGraph
{
    public class MainWindow : BaseToolWindow<MainWindow>
    {
        #region Nested type: Pane
        [Guid("aa2734ed-39b9-4dc2-8abf-43c57301a0f2")]
        public class Pane : ToolWindowPane
        {
            #region Constructors
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
            #endregion
        }
        #endregion

        #region Properties
        public override Type PaneType => typeof(Pane);
        #endregion

        #region Methods
        public override string GetTitle(int toolWindowId)
        {
            return "Refactor Graphs";
        }

        public override  Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new MainWindowControl());
        }
        #endregion
    }
}