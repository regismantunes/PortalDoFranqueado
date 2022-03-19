using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
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
