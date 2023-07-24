using System.Windows;
using System.Windows.Input;

namespace PortalDoFranqueado.ViewModel
{
    public interface IFieldViewModel
    {
        bool IsFocused { get; set; }
        int TabIndex { get; set; }
        object Tag { get; set; }

        event RoutedEventHandler LostFocus;
        event RoutedEventHandler GotFocus;

        ICommand OnGotFocus { get; }
        ICommand OnLostFocus { get; }
    }

    public interface IFieldViewModel<T> : IFieldViewModel
    {
        T Value { get; set; }
    }
}
