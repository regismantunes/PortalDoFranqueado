using PortalDoFranqueado.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
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
    }
}
