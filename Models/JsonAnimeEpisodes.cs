using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    public class JsonAnimeEpisodes
    {
        public Dictionary<string, JObject> tv { get; set; } = new Dictionary<string, JObject>()
        {
            { "360", new JObject()},
            { "480", new JObject()},
            { "720", new JObject()},
            { "1080", new JObject()}
        };

        public Dictionary<string, JObject> movies { get; set; } = new Dictionary<string, JObject>()
        {
            { "360", new JObject()},
            { "480", new JObject()},
            { "720", new JObject()},
            { "1080", new JObject()}
        };

        public Dictionary<string, JObject> ova { get; set; } = new Dictionary<string, JObject>()
        {
            { "360", new JObject()},
            { "480", new JObject()},
            { "720", new JObject()},
            { "1080", new JObject()}
        };

        public Dictionary<string, JObject> ona { get; set; } = new Dictionary<string, JObject>()
        {
            { "360", new JObject()},
            { "480", new JObject()},
            { "720", new JObject()},
            { "1080", new JObject()}
        };

        public Dictionary<string, JObject> ending { get; set; } = new Dictionary<string, JObject>()
        {
            { "360", new JObject()},
            { "480", new JObject()},
            { "720", new JObject()},
            { "1080", new JObject()}
        };

        public Dictionary<string, JObject> opening { get; set; } = new Dictionary<string, JObject>()
        {
            { "360", new JObject()},
            { "480", new JObject()},
            { "720", new JObject()},
            { "1080", new JObject()}
        };

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
