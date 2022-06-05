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
    public class ToolbarWindow : BaseToolWindow<ToolbarWindow>
    {
        #region Nested type: Pane
        [Guid("68D0E53E-82A5-48E2-AB8F-717D4DEBB604")]
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

        #region Fields
        private ToolbarWindowControl _control;
        #endregion

        #region Properties
        public ToolbarWindowControl Control => _control;
        public override Type PaneType => typeof(Pane);
        #endregion

        #region Methods
        public override string GetTitle(int toolWindowId)
        {
            return "Refactor Graph Toolbar";
        }

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            _control = new ToolbarWindowControl();
            return Task.FromResult<FrameworkElement>(_control);
        }
        #endregion
    }
}