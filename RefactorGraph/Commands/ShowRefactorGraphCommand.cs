using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace RefactorGraph
{
    [Command(PackageIds.ShowRefactorGraphCommand)]
    internal sealed class ShowRefactorGraphCommand : BaseCommand<ShowRefactorGraphCommand>
    {
        #region Methods
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return MainWindow.ShowAsync();
        }
        #endregion
    }
}