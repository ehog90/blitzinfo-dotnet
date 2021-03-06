﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class CountryImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var country = (string) value;
            if (country == "--") country = "empty";

            return @"pack://siteoforigin:,,,/Resources/" + country + "-01.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}