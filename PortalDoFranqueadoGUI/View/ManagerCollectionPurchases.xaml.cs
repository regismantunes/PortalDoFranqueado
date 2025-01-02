using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interação lógica para CollectionPurchase.xam
    /// </summary>
    public partial class ManagerCollectionPurchases : UserControl
    {
        public ManagerCollectionPurchases(Collection collection)
        {
            InitializeComponent();

            DataContext = new ManagerCollectionPurchasesViewModel(collection);
        }
    }
}
