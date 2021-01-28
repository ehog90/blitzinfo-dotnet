using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class BlitzDistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dst = (float) value;
            if (dst > 1000) return 1001;
            if (dst <= 1000 && dst > 200) return 201;
            if (dst <= 200 && dst > 50) return 51;
            if (dst <= 50 && dst > 20) return 21;
            if (dst <= 20 && dst > 5) return 6;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}