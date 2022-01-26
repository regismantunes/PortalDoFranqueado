using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para PurchaseCollection.xam
    /// </summary>
    public partial class CollectionPurchase : UserControl
    {
        public CollectionPurchase(int purchaseId)
        {
            InitializeComponent();

            DataContext = new CollectionPurchaseViewModel(purchaseId);
        }
    }
}
