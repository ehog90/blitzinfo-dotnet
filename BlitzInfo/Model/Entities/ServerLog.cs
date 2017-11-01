using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlitzInfo.Model.Entities
{
    public class ServerLogColor
    {
        [JsonProperty("bg")]
        public int Background { get; set; }
        [JsonProperty("fg")]
        public int Foreground { get; set; }
    }

    public class ServerLogMessageParts
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
        [JsonProperty("msgType")]
        public string MessageType { get; set; }

    }

    public class ServerLog
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("canBeHidden")]
        public bool CanBeHidden { get; set; }
        [JsonProperty("isError")]
        public bool IsAnError { get; set; }
        [JsonProperty("colors")]
        public ServerLogColor Colors { get; set; }
        [JsonProperty("messageParts")]
        public ServerLogMessageParts MessageParts { get; set; }
    }
}
