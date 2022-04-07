using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interação lógica para Login.xam
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();

            DataContext = new LoginViewModel();
        }
    }
}
