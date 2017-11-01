using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    class PercentComverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret;
            Double percent = Math.Round(System.Convert.ToDouble(value), 5);
           // if (percent > 1)
            {
                return percent.ToString() + " %";
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
