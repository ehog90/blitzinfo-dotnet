using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BlitzInfo.Model.Entities
{
    public class MetHuAlerts
    {
        public enum AreaTypes
        {
            COUNTY,
            REGIONAL_UNIT,
            UNKNOWN
        };

        [JsonProperty("type")]
        private string _areaType;

        [JsonProperty("name")]
        public string AreaName { get; set; }

        [JsonProperty("alerts")]
        public List<MeteoAlert> Alerts;

        public AreaTypes AreaType
        {
            get
            {
                switch (_areaType)
                {
                    case "county": return AreaTypes.COUNTY;
                    case "regionalUnit": return AreaTypes.REGIONAL_UNIT;
                    default: return AreaTypes.UNKNOWN;
                }
            }
        }
    }

    public class MeteoAlert
    {
        [JsonProperty("timeFirst")]
        private long _timeFirst;

        [JsonProperty("timeLast")]
        private long _timeLast;

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("alertType")]
        public string AlertType { get; set; }

        public DateTime TimeFirst
        {
            get
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(_timeFirst).ToLocalTime();
                return dtDateTime;
            }
        }


        public DateTime TimeLast
        {
            get
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(_timeLast).ToLocalTime();
                return dtDateTime;
            }
        }
    }
}
