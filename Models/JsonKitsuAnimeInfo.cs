using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonKitsuAnimeInfo
    {
#pragma warning disable IDE1006
        public string type { get; set; } = "kitsu_anime_info";
        public string anime_id { get; set; } = string.Empty;
        public int anime_episodes_per_page { get; set; } = 0;
        public int anime_total_episodes { get; set; } = 0;
        public int anime_total_episode_pages { get; set; } = 0;
        public JObject anime_info { get; set; } = new JObject();
        public JArray anime_relations { get; set; } = new JArray();
        public JArray anime_episodes { get; set; } = new JArray();
        public JArray anime_downloads { get; set; } = new JArray();
        public JObject anime_resolutions { get; set; } = new JObject();
        public JArray anime_categories { get; set; } = new JArray();
        public JArray anime_genres { get; set; } = new JArray();
        public JObject anime_rules { get; set; } = new JObject();
        public JObject anime_bot_sources { get; set; } = new JObject();
        public bool anime_stored { get; set; } = true;
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
