using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
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
