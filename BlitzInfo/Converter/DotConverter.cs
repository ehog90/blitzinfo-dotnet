﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    internal class DotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret;
            var count = System.Convert.ToInt64(value);
            // if (percent > 1)
            {
                return count + ".";
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