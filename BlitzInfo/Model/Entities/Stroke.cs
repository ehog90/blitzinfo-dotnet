using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OxyPlot;

namespace BlitzInfo.Model.Entities
{
    public class SunData
    {
        public double Azimuth { get; set; }
        public double Elevation { get; set; }
    }

    public class Stroke
    {
        public LatLon LatLon { get; set; }
        public string CountryCode { get; set; }
        public DateTime Time { get; set; }
        public string Region { get; set; }
        public string SubRegion { get; set; }
        public string Settlement { get; set; }
        public string Street { get; set; }
        public double Distance { get; set; }
        public string Direction { get; set; }
        public SunData SunData { get; set; }
        public double  Bearing { get; set; }

        public static Stroke GetBlitzFromJsonArray(object strokeObject)
        {
            JArray jArray = JArray.Parse(strokeObject.ToString());
            return new Stroke
            {
                LatLon =  new LatLon
                {
                    Latitude = (float)jArray[1][1],
                    Longitude = (float)jArray[1][0]
                },
                CountryCode = (string)jArray[2],
                Time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds((long)jArray[9]).ToLocalTime(),
                Region = (string)jArray[3],
                SubRegion = (string)jArray[4],
                Settlement = (string)jArray[5],
                Street = (string)jArray[6],
                SunData =  new SunData { Azimuth = (double)jArray[8], Elevation = (double)jArray[10] },
                Distance = 0

            };
        }

        public static List<Stroke> GetMultipleBlitzesFromJsonArray(object strokesObject)
        {
            List<Stroke> blitzes = new List<Stroke>();
            JArray jArray = JArray.Parse(strokesObject.ToString());
            foreach (var stroke in jArray.Children())
            {
                blitzes.Add(Stroke.GetBlitzFromJsonArray(stroke));
            }
            return blitzes;
        } 

    }
}
