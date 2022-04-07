using System;

namespace PortalDoFranqueado.ViewModel
{
    public interface INavigableViewModel : IDisposable
    {
        INavigatorViewModel Navigator { get; set; }
        
        void OnReturnToView();
    }
}
