using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BlitzInfo.Model.Entities
{
    public class StationStatEntry
    {
        [JsonProperty("sId")]
        public int StationId { get; set; }
        
        [JsonProperty("location")]
        public Address Location { get; set; }

        [JsonProperty("detCnt")]
        public long DetectionCount { get; set; }

        [JsonProperty("lastSeen")]
        public DateTime LastSeen { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }


        public static List<StationStatEntry> GetEntriesFromJson(object json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            return JsonConvert.DeserializeObject<List<StationStatEntry>>(json.ToString(),settings);
        }
    }
}
