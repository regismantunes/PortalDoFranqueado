using PortalDoFranqueado.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interação lógica para MainFranqueado.xam
    /// </summary>
    public partial class MainFranchisee : UserControl
    {
        public MainFranchisee()
        {
            InitializeComponent();

            DataContext = new MainFranchiseeViewModel()
            { StackPanelCampaigns = stpCampanhas };
        }
    }
}
