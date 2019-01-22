using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    class Jsonfullfilepath
    {
        public string type = "downloaded_files"; //used for identifying json
        public string directory { get; set; } = "/";

        public JsonAnimeInfo animeinfo { get; set; } = new JsonAnimeInfo();
        public List<JsonDownloadInfo> alreadyDownloaded { get; set; } = new List<JsonDownloadInfo>();

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
