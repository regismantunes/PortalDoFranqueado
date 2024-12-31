using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
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
