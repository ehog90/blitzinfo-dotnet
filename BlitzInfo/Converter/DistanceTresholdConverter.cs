using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    internal class DistanceTresholdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var distance = System.Convert.ToDouble(value);
            if (distance >= 3000) return "Mind";
            return string.Format("max. {0} km", distance);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}