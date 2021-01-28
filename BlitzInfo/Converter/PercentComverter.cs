using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    internal class PercentComverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret;
            var percent = Math.Round(System.Convert.ToDouble(value), 5);
            // if (percent > 1)
            {
                return percent + " %";
            }
            /*  else if (percent > 0)
              {
                  return (percent*10).ToString() + " ‰";
              }
              return "0 %";*/
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}