using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using BlitzInfo.Properties;
using Quobject.SocketIoClientDotNet.Client;

namespace BlitzInfo.Converter
{
    class CountryNameConverter : IValueConverter
    {

        private string GetResxNameByValue(string value)
        {
            return  Resources.ResourceManager.GetString(String.Format("iso3166_{0}",value));

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string countryCode = (string) value;
            string countryName = GetResxNameByValue(countryCode);
            if (countryName != null)
            {
                return countryName;
            }
            return "Ismeretlen";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
