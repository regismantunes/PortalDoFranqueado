using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interaction logic for ManagerCampaigns.xaml
    /// </summary>
    public partial class ManagerCampaigns : UserControl
    {
        public ManagerCampaigns()
        {
            InitializeComponent();

            DataContext = new ManagerCampaignsViewModel();
        }
    }
}
