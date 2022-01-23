using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interação lógica para ManagerCompras.xam
    /// </summary>
    public partial class ManagerCollections : UserControl
    {
        public ManagerCollections()
        {
            InitializeComponent();

            DataContext = new ManagerCollectionsViewModel();
        }
    }
}
