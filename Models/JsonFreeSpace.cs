using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LittleWeebLibrary.Models
{
    class JsonFreeSpace
    {
        public string type = "free_space";
        public long freespacebytes { get; set; } = -1;
        public long freespacekbytes { get; set; } = -1;
        public long freespacembytes { get; set; } = -1;
        public string updated { get; set; } = DateTime.Now.Millisecond.ToString();

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
