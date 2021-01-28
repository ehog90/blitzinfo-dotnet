using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    internal class SoundTresholdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var distance = System.Convert.ToDouble(value);
            if (distance >= 1000)
                return "Mind";
            if (distance <= 0) return "Semelyiket";
            return string.Format("max. {0} km", distance);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}