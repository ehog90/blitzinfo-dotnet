using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    public class DistanceBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double dist = System.Convert.ToDouble(value);
            String urisource;

            if (dist > 1000)
            {
                urisource = @"pack://application:,,,/Resources/dot1000plus.png";
            }
            else if (dist <= 1000 && dist > 200)
            {
                urisource = @"pack://application:,,,/Resources/dot200_1000.png";
            }
            else if (dist <= 200 && dist > 50)
            {
                urisource = @"pack://application:,,,/Resources/dot50_200.png";
            }
            else if (dist <= 50 && dist > 20)
            {
                urisource = @"pack://application:,,,/Resources/dot20_50.png";
            }
            else if (dist <= 20 && dist > 5)
            {
                urisource = @"pack://application:,,,/Resources/dot5_20.png";
            }
            else if (dist <= 5)
            {
                urisource = @"pack://application:,,,/Resources/dot0_5.png";
            }
            else
            {
                urisource = @"pack://application:,,,/Resources/dot0_5.png";
            }
            return urisource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
