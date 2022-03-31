using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PortalDoFranqueadoGUI.Util.Converter
{
    public class ExpandableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is IExpandable expandable)
                return expandable.IsExpanded;

            if (value is IEnumerable<IExpandable> enumerable)
                return enumerable.FirstOrDefault()?.IsExpanded ?? false;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
