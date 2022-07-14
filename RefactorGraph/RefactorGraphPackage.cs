using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace RefactorGraph
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MainWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideToolWindow(typeof(DesignerWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.MainWindow)]
    [ProvideToolWindow(typeof(ToolbarWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.MainWindow)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.RefactorGraphGuidString)]
    public sealed class RefactorGraphPackage : ToolkitPackage
    {
        #region Methods
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Utils.SetMdStyle();
            
            MainWindow.Initialize(this);
            DesignerWindow.Initialize(this);
            ToolbarWindow.Initialize(this);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ShowRefactorGraphCommand.InitializeAsync(this);
        }
        #endregion
    }
}