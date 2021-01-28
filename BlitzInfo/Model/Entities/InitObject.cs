using Newtonsoft.Json;

namespace BlitzInfo.Model
{
    internal class InitObject
    {
        [JsonProperty("lang")] public static string LANGUAGE = "hun";

        [JsonProperty("latLon")] public float[] LatLon { get; set; }

        [JsonProperty("dtr")] public int DistanceTreshold { get; set; }

        [JsonProperty("dirs")] public int[] Directions { get; set; }

        [JsonProperty("dt")] public string DeviceType { get; set; }
    }
}