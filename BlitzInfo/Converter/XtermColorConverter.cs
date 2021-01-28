using System;
using System.Globalization;
using System.Windows.Data;
using BlitzInfo.Properties;

namespace BlitzInfo.Converter
{
    internal class XtermColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorString = GetResxNameByValue((int) value);
            return colorString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }

        private string GetResxNameByValue(int value)
        {
            return Resources.ResourceManager.GetString($"xterm_{value}");
        }
    }
}