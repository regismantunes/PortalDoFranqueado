using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para PurchaseCollection.xam
    /// </summary>
    public partial class CollectionPurchase : UserControl
    {
        public CollectionPurchase(Purchase purchase)
        {
            InitializeComponent();

            DataContext = new CollectionPurchaseViewModel(purchase);
        }
    }
}
