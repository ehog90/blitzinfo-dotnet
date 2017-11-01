using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using BlitzInfo.Model.Entities;

namespace BlitzInfo.Converter
{
    public class SunElevationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SunData sunData = (SunData)value;


            String changing, state;

            if (sunData.Azimuth > 178 || sunData.Azimuth < -178)
            {
                changing = "éjközép";
            }
            else if ((sunData.Azimuth > -2 && sunData.Azimuth <= 2) || (sunData.Azimuth < 2 && sunData.Azimuth > -1))
            {
                changing = "delel";
            }
            else if (sunData.Azimuth < 0)
            {
                changing = "emelkedik";
            }
            else
            {
                changing = "süllyed";
            }

            if (sunData.Elevation >= 0.5)
            {
                state = "nappal";
            }
            else if (sunData.Elevation >= -0.5)
            {
                if (sunData.Azimuth < 0)
                {
                    state = "napnyugta";
                }
                state = "napkelte";
            }
            else if (sunData.Elevation >= -5.0)
            {
                state = "polgári szürkület";
            }
            else if (sunData.Elevation >= -12.0)
            {
                state = "navigációs szürkület";
            }
            else if (sunData.Elevation >= -18.0)
            {
                state = "csillagászati szürkület";
            }
            else
            {
                state = "éjszaka";
            }

            return String.Format("{0}° - {1} ({2})", sunData.Elevation, state, changing);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
