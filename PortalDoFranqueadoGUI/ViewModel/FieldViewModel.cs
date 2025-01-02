using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PortalDoFranqueado.ViewModel
{
    public class FieldViewModel<T> : BaseNotifyPropertyChanged, IFieldViewModel<T>
    {
        private T _value;
        private bool _isfocused;
        private int _tabIndex;

        public FieldViewModel()
        {
            OnLostFocus = new RelayCommand(() => IsFocused = false);
            OnGotFocus = new RelayCommand(() => IsFocused = true);
        }

        public FieldViewModel(T value)
            : this()
        {
            Value = value;
        }

        public T Value
        {
            get => _value;
            set 
            {   
                _value = value;

                if (_value is INotifyPropertyChanged notify)
                    notify.PropertyChanged += (o, e) => OnPropertyChanged(e.PropertyName);

                OnPropertyChanged(); 
            }
        }

        public bool IsFocused
        {
            get => _isfocused;
            set 
            {
                if (_isfocused == value)
                    return;

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

        public object Tag { get; set; }

        public event RoutedEventHandler LostFocus;
        public event RoutedEventHandler GotFocus;

        public ICommand OnGotFocus { get; }
        public ICommand OnLostFocus { get; }
    }
}
