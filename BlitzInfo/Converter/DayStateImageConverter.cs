using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class DayStateImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var elevation = (double) value;
            if (elevation >= 0.5) return @"pack://application:,,,/Resources/d_daylight.png";
            if (elevation >= -0.5) return @"pack://application:,,,/Resources/d_horizon.png";
            if (elevation >= -5.0) return @"pack://application:,,,/Resources/d_civil.png";
            if (elevation >= -12.0) return @"pack://application:,,,/Resources/d_nautical.png";
            if (elevation >= -18.0) return @"pack://application:,,,/Resources/d_astronomical.png";
            return @"pack://application:,,,/Resources/d_night.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}