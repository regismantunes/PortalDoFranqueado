using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public class FieldViewModel<T> : BaseNotifyPropertyChanged
    {
        private T _value;
        private bool _isfocused;
        private int _tabIndex;

        public FieldViewModel()
        {
            OnLostFocus = new RelayCommand(() => IsFocused = false);
            OnGotFocus = new RelayCommand(() => IsFocused = true);
        }

        public T Value
        {
            get => _value;
            set 
            {   
                _value = value;

                if (_value is INotifyPropertyChanged notify)
                    notify.PropertyChanged += (o, e) => OnPropertyChanged(nameof(Value));

                OnPropertyChanged(); 
            }
        }

        public bool IsFocused
        {
            get => _isfocused;
            set 
            { 
                _isfocused = value; 
                OnPropertyChanged();

                if (_isfocused)
                    GotFocus?.Invoke(this, null);
                else
                    LostFocus?.Invoke(this, null);
            }
        }

        public int TabIndex
        {
            get => _tabIndex;
            set { _tabIndex = value; OnPropertyChanged(); }
        }

        public event RoutedEventHandler LostFocus;
        public event RoutedEventHandler GotFocus;

        public RelayCommand OnGotFocus { get; }
        public RelayCommand OnLostFocus { get; }
    }
}
