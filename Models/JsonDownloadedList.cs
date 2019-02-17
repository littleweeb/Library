using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LittleWeebLibrary.Models
{
    public class JsonDownloadedList
    {
#pragma warning disable IDE1006
        public string type { get; set; } = "download_history_list";
        public string anime_id { get; set; } = string.Empty;
        public string anime_title { get; set; } = string.Empty;
        public JObject anime_cover { get; set; } = new JObject();
        public JArray downloadHistorylist { get; set; } = new JArray();
        public string updated { get; set; } = StaticClasses.UtilityMethods.GetEpoch().ToString();
#pragma warning restore IDE1006

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
