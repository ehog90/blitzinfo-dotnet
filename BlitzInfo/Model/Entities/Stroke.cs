using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        public double Bearing { get; set; }

        public static Stroke GetBlitzFromJsonArray(object strokeObject)
        {
            var jArray = JArray.Parse(strokeObject.ToString());
            return new Stroke
            {
                LatLon = new LatLon
                {
                    Latitude = (float) jArray[1][1],
                    Longitude = (float) jArray[1][0]
                },
                CountryCode = (string) jArray[2],
                Time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds((long) jArray[9]).ToLocalTime(),
                Region = (string) jArray[3],
                SubRegion = (string) jArray[4],
                Settlement = (string) jArray[5],
                Street = (string) jArray[6],
                SunData = new SunData {Azimuth = (double) jArray[8], Elevation = (double) jArray[10]},
                Distance = 0
            };
        }

        public static List<Stroke> GetMultipleBlitzesFromJsonArray(object strokesObject)
        {
            var blitzes = new List<Stroke>();
            var jArray = JArray.Parse(strokesObject.ToString());
            foreach (var stroke in jArray.Children()) blitzes.Add(GetBlitzFromJsonArray(stroke));
            return blitzes;
        }
    }
}