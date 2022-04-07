using PortalDoFranqueado.Model;
using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interaction logic for ManagerPhotos.xaml
    /// </summary>
    public partial class ManagerFiles : UserControl
    {
        public ManagerFiles(FileOwner ownerType, int id, string title)
        {
            InitializeComponent();

            DataContext = new ManagerFilesViewModel(ownerType, id, title);
        }
    }
}
