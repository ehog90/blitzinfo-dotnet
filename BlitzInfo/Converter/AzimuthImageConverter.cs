using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class AzimuthImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var azimuth = (double) value;
            if (azimuth > 178 || azimuth < -178)
                return @"pack://application:,,,/Resources/uparrow_o_min.png";
            if (azimuth > -2 && azimuth <= 2 || azimuth < 2 && azimuth > -1)
                return @"pack://application:,,,/Resources/downarrow_o_max.png";
            if (azimuth < 0)
                return @"pack://application:,,,/Resources/uparrow_o.png";
            return @"pack://application:,,,/Resources/downarrow_o.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}