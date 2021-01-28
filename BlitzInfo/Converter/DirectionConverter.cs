using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    internal class DirectionConverter : IValueConverter
    {
        private static readonly string[] values =
        {
            "É", "ÉÉK", "ÉK", "KÉK", "K", "KDK", "DK", "DDK", "D", "DDNy", "DNy", "NyDNy", "Ny", "NyÉNy", "ÉNy", "ÉÉNy",
            "É"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Ismeretlen";
            var bearing = (double) value;
            var index = System.Convert.ToInt32(Math.Round(bearing / 22.5));
            return values[index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}