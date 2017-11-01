using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using BlitzInfo.Properties;

namespace BlitzInfo.Converter
{
    class XtermColorConverter : IValueConverter
    {
        private string GetResxNameByValue(int value)
        {
            return Resources.ResourceManager.GetString(String.Format("xterm_{0}", value));

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String colorString = GetResxNameByValue((int) value);
            return colorString;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
