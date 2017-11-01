using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Navigation;

namespace BlitzInfo.Converter
{
    public class KilometerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret;
            Double km = Math.Round(System.Convert.ToDouble(value),1);
            if (km > 2)
            {
                return km.ToString() + " km";
            }
            return (km*1000).ToString() + " méter";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}