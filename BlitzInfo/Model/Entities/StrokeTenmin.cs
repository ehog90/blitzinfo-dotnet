using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlitzInfo.Model.Entities
{
    public class StrokeTenmin
    {
        public class BlitzCountCountry
        {
            public string countrycode { get; set; }
            public int countrycount { get; set; }
        }
        public DateTime time { get; set; }
        public string nicetime { get; set; }
        public int count { get; set; }
        public List<BlitzCountCountry> country_data;

        public static StrokeTenmin GetTenminDataFromJsonArray(object tenminObject)
        {
            //return new StrokeTenmin {time = DateTime.Now, nicetime= "lofasz", count= 420; country_data = new };
            JArray jArray = JArray.Parse(tenminObject.ToString());
            JArray countriesArray = (JArray)jArray[2];
            List<BlitzCountCountry> countryData = (from countryCount in countriesArray.Children()
                let countryCode = (string) countryCount[0]
                let cnt = (int) countryCount[1]
                select new BlitzCountCountry
                {
                    countrycode = (string) countryCount[0], countrycount = (int) countryCount[1]
                }).ToList();


            return new StrokeTenmin
            {
                time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds((long)jArray[0]).ToLocalTime(),
                nicetime =  "",
                count = (int)jArray[1],
                country_data = countryData
            };
        }


        public static List<StrokeTenmin> GetMultipleTenminDataFromJsonArray(object tenminsObject)
        {
            List<StrokeTenmin> strokeTenmins = new List<StrokeTenmin>();
            JArray jArray = JArray.Parse(tenminsObject.ToString());
            foreach (var stroke in jArray.Children())
            {
                strokeTenmins.Add(StrokeTenmin.GetTenminDataFromJsonArray(stroke));
            }
            return strokeTenmins;
        }
    }

}
