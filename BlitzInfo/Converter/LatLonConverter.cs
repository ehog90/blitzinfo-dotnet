using System;
using System.Globalization;
using System.Windows.Data;
using BlitzInfo.Model.Entities;

namespace BlitzInfo.Converter
{
    public class LatLonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var latLon = (LatLon) value;
            string lattext, lontext;
            if (latLon.Latitude > 0)
                lattext = "É";
            else
                lattext = "D";
            if (latLon.Longitude > 0)
                lontext = "K";
            else
                lontext = "Ny";

            return string.Format("{0}° {1}, {2}° {3}", latLon.Latitude, lattext, latLon.Longitude, lontext);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}