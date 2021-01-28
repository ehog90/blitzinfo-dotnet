using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class BlitzNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = (int) value;
            return " - " + count + " villám";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}