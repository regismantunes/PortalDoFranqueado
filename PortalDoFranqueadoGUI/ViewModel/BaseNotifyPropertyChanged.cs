using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortalDoFranqueado.ViewModel
{
    public abstract class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
