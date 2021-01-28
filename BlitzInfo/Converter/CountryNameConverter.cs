using System;
using System.Globalization;
using System.Windows.Data;
using BlitzInfo.Properties;

namespace BlitzInfo.Converter
{
    internal class CountryNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var countryCode = (string) value;
            var countryName = GetResxNameByValue(countryCode);
            if (countryName != null) return countryName;
            return "Ismeretlen";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }

        private string GetResxNameByValue(string value)
        {
            return Resources.ResourceManager.GetString($"iso3166_{value}");
        }
    }
}