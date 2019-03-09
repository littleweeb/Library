using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonDownloadedList
    {
#pragma warning disable IDE1006
        public string type { get; set; } = "download_history_list";
        public List<JObject> downloaded_anime = new List<JObject>();
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
