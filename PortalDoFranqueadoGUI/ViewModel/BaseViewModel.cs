using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public abstract class BaseViewModel : BaseNotifyPropertyChanged, INavigableViewModel
    {
        public event Action? CloseRequest;
        public INavigatorViewModel Navigator { get; set; }

        private string _contentLocker = string.Empty;
        private Cursor _cursor = Cursors.Arrow;

        public Cursor CurrentCursor
        {
            get => _cursor;
            set { _cursor = value; OnPropertyChanged(); }
        }
        public bool EnabledContent { get; private set; } = true;

        public Window? Me { get; set; }
        private ImageSource? _icon;
        public ImageSource? Icon
        {
            get
            {
                if (_icon != null)
                    return _icon;
                else if (Me != null)
                    return Me.Owner.Icon;
                else
                    return null;
            }
            set { _icon = value; OnPropertyChanged(); }
        }

        public void DesableContent([CallerMemberName] string? contentLocker = null)
        {
            lock (_contentLocker)
            {
                _contentLocker = contentLocker ?? "";
                EnabledContent = false;
                OnPropertyChanged(nameof(EnabledContent));
                CurrentCursor = Cursors.Wait;
            }
        }

        public void EnableContent([CallerMemberName] string? contentLocker = null)
        {
            lock (_contentLocker)
            {
                if (_contentLocker == (contentLocker ?? string.Empty))
                {
                    _contentLocker = "";
                    EnabledContent = true;
                    OnPropertyChanged(nameof(EnabledContent));
                    CurrentCursor = Cursors.Arrow;
                }
            }
        }

        public void SetValue<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
        {
            if (property != null &&
                property.Equals(value))
                return;

            OnPropertyChanged(propertyName);
            property = value;
        }

        protected void OnCloseRequest() => CloseRequest?.Invoke();

        public virtual void OnReturnToView()
        {
            if (this is IReloadable reloadable)
                reloadable.Reload();
        }

        public void Dispose()
        {
            CloseRequest?.Invoke();
            GC.SuppressFinalize(this);
        }
    }
}
