using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BlitzInfo.Model.Entities
{
    public class Address
    {
        [JsonProperty("strDef")]
        public string Street { get; set; }

        [JsonProperty("suburbDef")]
        public string Suburb { get; set; }

        [JsonProperty("smDef")]
        public string Settlement { get; set; }

        [JsonProperty("sRegDef")]
        public string Subregion { get; set; }

        [JsonProperty("regDef")]
        public string Region { get; set; }

        [JsonProperty("cc")]
        public string CountryCode { get; set; }
    }
}
