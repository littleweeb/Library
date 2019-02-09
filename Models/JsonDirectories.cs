using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonDirectories
    {
        public string type { get; set; } = "directories";
        public List<JsonDirectory> directories = new List<JsonDirectory>();
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
