using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    public class JsonIrcInfo
    {
        public string type = "irc_data";
        public bool connected { get; set; } = false;
        public string channel { get; set; } = string.Empty;
        public string server { get; set; } = string.Empty;
        public string user { get; set; } = string.Empty;
        public string fullfilepath{ get; set; } = string.Empty;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public override string ToString()
        {
            JObject jobject = JObject.FromObject(this);
            string properties = string.Empty;
            foreach (var x in jobject)
            {
                properties += x.Key + ": " + x.Value.ToString();
            }
            return properties;
        }
    }
}
