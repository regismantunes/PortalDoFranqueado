using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para FranquadoCompra.xam
    /// </summary>
    public partial class PurchaseStore : UserControl
    {
        public PurchaseStore()
        {
            InitializeComponent();

            DataContext = new PurchaseStoreViewModel();
        }
    }
}
