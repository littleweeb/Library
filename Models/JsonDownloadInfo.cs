using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    public class JsonDownloadInfo
    {
        public string type = "download_update"; //used for identifying json
        public JsonAnimeInfo animeInfo { get; set; } = new JsonAnimeInfo();
        public string id { get; set; } = string.Empty;
        public string episodeNumber { get; set; } = string.Empty;
        public string bot { get; set; } = string.Empty;
        public string pack { get; set; } = string.Empty;
        public string progress { get; set; } = string.Empty;
        public string speed { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string filesize { get; set; } = string.Empty;
        public int downloadIndex { get; set; } = -1;
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
