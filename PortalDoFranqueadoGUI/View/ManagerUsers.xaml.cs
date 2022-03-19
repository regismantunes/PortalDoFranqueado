using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interaction logic for ManagerUsers.xaml
    /// </summary>
    public partial class ManagerUsers : UserControl
    {
        public ManagerUsers()
        {
            InitializeComponent();

            DataContext = new ManagerUsersViewModel();
        }
    }
}
