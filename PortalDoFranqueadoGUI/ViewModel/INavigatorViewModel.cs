using System.Windows.Controls;

namespace PortalDoFranqueado.ViewModel
{
    public interface INavigatorViewModel
    {
        void NavigateTo(ContentControl control);
        ContentControl ReturnNavigation();
    }
}