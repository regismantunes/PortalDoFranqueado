using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public interface INavigatorViewModel
    {
        void NavigateTo(ContentControl control);
        ContentControl ReturnNavigation();
    }
}