using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
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
