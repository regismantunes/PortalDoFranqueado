using System.ComponentModel;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class FieldViewModel<T> : BaseNotifyPropertyChanged
    {
        private T _value;
        private bool _isfocused;

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
            set { _isfocused = value; OnPropertyChanged(); }
        }
    }
}
