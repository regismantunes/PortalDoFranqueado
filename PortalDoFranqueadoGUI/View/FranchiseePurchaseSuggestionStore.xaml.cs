using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interaction logic for FranchiseePurchaseSuggestionStore.xaml
    /// </summary>
    public partial class FranchiseePurchaseSuggestionStore : UserControl
    {
        public FranchiseePurchaseSuggestionStore()
        {
            InitializeComponent();

            DataContext = new FranchiseePurchaseSuggestionStoreViewModel();
        }
    }
}
