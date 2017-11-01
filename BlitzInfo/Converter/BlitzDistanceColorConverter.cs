using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class BlitzDistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float dst = (float)value;
            if (dst > 1000) return 1001;
            else if (dst <= 1000 && dst > 200) return 201;
            else if (dst <= 200 && dst > 50) return 51;
            else if (dst <= 50 && dst > 20) return 21;
            else if (dst <= 20 && dst > 5) return 6;
            else return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
