using PortalDoFranqueadoGUI.Update;
using System.Windows;

namespace PortalDoFranqueadoGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Updater.Initialize();
        }
    }
}
