using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret;
            Int64 count = System.Convert.ToInt64(value);
            // if (percent > 1)
            {
                return count.ToString("##,#") + " db";
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
