using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
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
