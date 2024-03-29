﻿using System;
using System.Windows;
using System.Windows.Data;
using PortalDoFranqueado.Util.Extensions;

namespace PortalDoFranqueado.Util.Converter
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
