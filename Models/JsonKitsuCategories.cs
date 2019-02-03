using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonKistuGenres
    {
        public string type { get; set; } = "kitsu_genres";
        public JArray result { get; set; } = new JArray();

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
