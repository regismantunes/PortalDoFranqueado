using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para CollectionPurchase.xam
    /// </summary>
    public partial class CollectionPurchases : UserControl
    {
        public CollectionPurchases(Collection collection)
        {
            InitializeComponent();

            DataContext = new CollectionPurchasesViewModel(collection);
        }
    }
}
