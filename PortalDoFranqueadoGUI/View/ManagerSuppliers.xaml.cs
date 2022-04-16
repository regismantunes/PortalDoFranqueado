using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interaction logic for ManagerSupplier.xaml
    /// </summary>
    public partial class ManagerSuppliers : UserControl
    {
        public ManagerSuppliers()
        {
            InitializeComponent();

            this.DataContext = new ManagerSuppliersViewModel();
        }
    }
}
