using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LittleWeebLibrary.Models
{
    public class JsonKitsuAnimeInfo
    {
        public string type { get; set; } = "kitsu_anime_info";
        public JObject anime_info { get; set; } = new JObject();
        public JArray anime_relations { get; set; } = new JArray();
        public JArray anime_episodes { get; set; } = new JArray();
        public JArray anime_categories { get; set; } = new JArray();
        public JArray anime_genres { get; set; } = new JArray();

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
