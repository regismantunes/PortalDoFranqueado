using System.Windows.Controls;

namespace PortalDoFranqueado.ViewModel
{
    public interface INavigatorViewModel
    {
        void NavigateTo(ContentControl control);
        bool ReturnNavigation();
    }
}