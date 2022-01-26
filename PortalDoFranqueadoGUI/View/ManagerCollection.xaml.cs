using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para ManagerCompra.xam
    /// </summary>
    public partial class ManagerCollection : UserControl
    {
        public ManagerCollection(Collection collection, bool canEdit)
        {
            InitializeComponent();

            DataContext = new ManagerCollectionViewModel(collection, canEdit);
        }
    }
}
