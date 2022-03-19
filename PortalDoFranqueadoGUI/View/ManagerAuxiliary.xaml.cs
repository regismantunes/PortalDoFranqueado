using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interaction logic for ManagerPhotos.xaml
    /// </summary>
    public partial class ManagerAuxiliary : UserControl
    {
        public ManagerAuxiliary(FileOwner ownerType, int id, string title)
        {
            InitializeComponent();

            DataContext = new ManagerAuxiliaryViewModel(ownerType, id, title);
        }
    }
}
