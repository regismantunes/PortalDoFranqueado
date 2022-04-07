using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interação lógica para FranquadoCompra.xam
    /// </summary>
    public partial class FranchiseePurchaseStore : UserControl
    {
        public FranchiseePurchaseStore()
        {
            InitializeComponent();

            DataContext = new FranchiseePurchaseStoreViewModel();
        }
    }
}
