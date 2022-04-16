using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interaction logic for ManagerStores.xaml
    /// </summary>
    public partial class ManagerStores : UserControl
    {
        public ManagerStores()
        {
            InitializeComponent();

            this.DataContext = new ManagerStoresViewModel();
        }
    }
}
