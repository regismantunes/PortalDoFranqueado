namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class FieldViewModel<T> : BaseNotifyPropertyChanged
    {
        private T _value;
        private bool _isfocused;

        public T Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public bool IsFocused
        {
            get => _isfocused;
            set { _isfocused = value; OnPropertyChanged(); }
        }
    }
}
