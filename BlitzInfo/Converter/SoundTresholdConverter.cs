using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlitzInfo.Converter
{
    class SoundTresholdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double distance = System.Convert.ToDouble(value);
            if (distance >= 1000)
            {
                return "Mind";
            }
            else if (distance <= 0)
            {
                return "Semelyiket";
            }
            return String.Format("max. {0} km", distance);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
