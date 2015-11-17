using System.Windows;
using DevExpress.Xpf.Core;

namespace Merge.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.ApplicationThemeName = Theme.MetropolisLightName;
            base.OnStartup(e);
        }
    }
}
