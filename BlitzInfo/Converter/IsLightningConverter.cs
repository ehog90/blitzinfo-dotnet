using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class IsLightningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime) value;
            var difference = DateTime.Now - dateTime;
            if (difference.TotalMinutes < 5) return @"pack://application:,,,/Resources/lightning-01.png";
            return @"pack://application:,,,/Resources/nolightning-01.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}