using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LittleWeebLibrary.Models
{
    public class JsonCurrentlyAiring
    {
        public string type { get; set; } = "anime_info_currently_airing";
        public JObject result { get; set; } = new JObject();

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
