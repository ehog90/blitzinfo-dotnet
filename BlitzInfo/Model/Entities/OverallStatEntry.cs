using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlitzInfo.Model.Entities
{
    public class OverallStatEntry
    {
        public int CountryRank { get; set; }
        public string CountryCode { get; set; }
        public long CountryCount { get; set; }
        public float CountryPercent { get; set; }
        public DateTime CountryLastAdded { get; set; }

        public static OverallStatEntry GetStatEntryFromJsonArray(object overallStatObject)
        {
            JArray jArray = JArray.Parse(overallStatObject.ToString());
            return new OverallStatEntry
            {
                CountryCode = (string) jArray[0],
                CountryLastAdded = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds((long)jArray[2]).ToLocalTime(),
                CountryCount = (int) jArray[1],
                CountryPercent = (float) jArray[3],
            };
        }

        public static List<OverallStatEntry> GetMultipleStatEntryFromJsonArray(object overallStatsObject)
        {
            List<OverallStatEntry> overallStatEntries = new List<OverallStatEntry>();
            JArray jArray = JArray.Parse(overallStatsObject.ToString());
            foreach (var overallStatObject in jArray.Children())
            {
                overallStatEntries.Add(GetStatEntryFromJsonArray(overallStatObject));
            }
            int n = 1;
            overallStatEntries = overallStatEntries.OrderByDescending(x => x.CountryCount).ToList();
            foreach (var overallStatEntry in overallStatEntries)
            {
                overallStatEntry.CountryRank = n;
                ++n;
            }

            return overallStatEntries;
        }
    }
}
