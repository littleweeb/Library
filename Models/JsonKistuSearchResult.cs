using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LittleWeebLibrary.Models
{
    public class JsonKistuSearchResult
    {
        public string type { get; set; } = "kitsu_search_result";
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
