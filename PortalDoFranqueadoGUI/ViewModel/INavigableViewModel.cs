using System;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public interface INavigableViewModel : IDisposable
    {
        INavigatorViewModel Navigator { get; set; }
        
        void OnReturnToView();
    }
}
