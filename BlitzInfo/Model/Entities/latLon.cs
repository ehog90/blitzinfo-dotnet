using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlitzInfo.Model.Entities
{
    public class LatLon
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public static double Distance(LatLon latLon1, LatLon latLon2)
        {
            double theta = latLon1.Longitude - latLon2.Longitude;
            double dist = Math.Sin(deg2rad(latLon1.Latitude)) * Math.Sin(deg2rad(latLon2.Latitude)) + Math.Cos(deg2rad(latLon1.Latitude)) * Math.Cos(deg2rad(latLon2.Latitude)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            dist = dist * 1.609344;
            return (dist);
        }

        public static double Bearing(LatLon from, LatLon to)
        {
            double startLat = deg2rad(from.Latitude);
            double startLong = deg2rad(from.Longitude);
            double endLat = deg2rad(to.Latitude);
            double endLong = deg2rad(to.Longitude);

            double dLong = endLong - startLong;
            double dPhi = Math.Log(Math.Tan(endLat / 2.0 + Math.PI / 4.0) / Math.Tan(startLat / 2.0 + Math.PI / 4.0));
            if (Math.Abs(dLong) > Math.PI)
            {
                if (dLong > 0.0)
                    dLong = -(2.0 * Math.PI - dLong);
                else
                    dLong = (2.0 * Math.PI + dLong);
            }

            return (rad2deg(Math.Atan2(dLong, dPhi)) + 360.0) % 360.0;
        }

        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}
