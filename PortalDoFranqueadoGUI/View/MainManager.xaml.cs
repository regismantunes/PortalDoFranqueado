using PortalDoFranqueadoGUI.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para MainManager.xam
    /// </summary>
    public partial class MainManager : UserControl
    {
        public MainManager()
        {
            InitializeComponent();

            DataContext = new MainManagerViewModel();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainManagerViewModel)DataContext).SetWindow(Window.GetWindow(this));
        }
    }
}
