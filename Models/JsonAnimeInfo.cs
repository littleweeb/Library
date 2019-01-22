using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonAnimeInfo
    {
        public string type { get; set; } = "anime_info";
        public string anime_id { get; set; } = "";
        public string anime_title { get; set; } = "";
        public List<string> anime_synonyms { get; set; } = new List<string>();
        public string anime_cover_original { get; set; } = "";
        public string anime_cover_small { get; set; } = "";
        public string anime_synopsis { get; set; } = "";
        public int anime_season { get; set; } = 0;
        public int anime_episode_count { get; set; } = 0;
        public string anime_score { get; set; } = "";
        public string anime_status { get; set; } = "";
        public string anime_episodeLength { get; set; } = "";
        public string anime_type { get; set; } = "";
        public JsonAnimeEpisodes anime_episodes { get; set; } = new JsonAnimeEpisodes();
        public Dictionary<string, string> anime_related { get; set; } = new Dictionary<string, string>(); //key = title, value = id.

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
