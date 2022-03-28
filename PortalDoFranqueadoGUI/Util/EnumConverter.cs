using System;
using System.Windows;
using System.Windows.Data;
using PortalDoFranqueadoGUI.Util.Extensions;

namespace PortalDoFranqueadoGUI.Util
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) 
                return DependencyProperty.UnsetValue;

            return ((Enum)value).GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
