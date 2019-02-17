using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LittleWeebLibrary.Models
{
    public class JsonCurrentlyAiring
    {
#pragma warning disable IDE1006
        public string type { get; set; } = "anime_info_currently_airing";
        public JObject result { get; set; } = new JObject();
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
