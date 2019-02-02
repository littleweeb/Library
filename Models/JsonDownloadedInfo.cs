using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonDownloadedInfo
    {
        public string type = "downloaded_file";
        public string anime_id { get; set; } = string.Empty;
        public string anime_name { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string season { get; set; } = string.Empty;
        public string episodeNumber { get; set; } = string.Empty;
        public string bot { get; set; } = string.Empty;
        public string pack { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string filesize { get; set; } = string.Empty;
        public string fullfilepath { get; set; } = string.Empty;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public override string ToString()
        {
            JObject jobject = ToJObject();
            string properties = string.Empty;
            foreach (var x in jobject)
            {
                properties += x.Key + ": " + x.Value.ToString();
            }
            return properties;
        }

        public JObject ToJObject()
        {

            JObject jobject = JObject.FromObject(this);
            return jobject;
        }
    }
}
