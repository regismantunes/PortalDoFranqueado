using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para PurchaseCollection.xam
    /// </summary>
    public partial class ManagerCollectionPurchase : UserControl
    {
        public ManagerCollectionPurchase(Purchase purchase)
        {
            InitializeComponent();

            DataContext = new ManagerCollectionPurchaseViewModel(purchase);
        }
    }
}
