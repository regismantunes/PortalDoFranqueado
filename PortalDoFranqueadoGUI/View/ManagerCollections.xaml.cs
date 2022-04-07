using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
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
