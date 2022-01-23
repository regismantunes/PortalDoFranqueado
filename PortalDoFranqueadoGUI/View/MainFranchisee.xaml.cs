using PortalDoFranqueadoGUI.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainFranchiseeViewModel)DataContext).SetWindow(Window.GetWindow(this));
        }
    }
}
