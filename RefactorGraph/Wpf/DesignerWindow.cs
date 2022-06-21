using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using NodeGraph.ViewModel;

namespace RefactorGraph
{
    public class DesignerWindow : BaseToolWindow<DesignerWindow>
    {
        #region Nested type: Pane
        [Guid("98AD56B1-8553-4A19-AD2F-10837780492A")]
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
        private DesignerWindowControl _control;
        #endregion

        #region Properties
        public DesignerWindowControl Control => _control;
        public override Type PaneType => typeof(Pane);
        #endregion

        #region Methods
        public override string GetTitle(int toolWindowId)
        {
            return "Refactor Graph Canvas";
        }

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            _control = new DesignerWindowControl();
            return Task.FromResult<FrameworkElement>(_control);
        }

        public void SetFlowChartViewModel(FlowChartViewModel flowChartViewModel)
        {
            _control.FlowChartViewModel = flowChartViewModel;
        }

        #endregion
    }
}