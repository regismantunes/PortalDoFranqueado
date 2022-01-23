using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public interface INavigatorViewModel
    {
        void NextNavigate(ContentControl control);
        ContentControl PreviousNavigate();
    }
}