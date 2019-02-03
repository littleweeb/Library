using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LittleWeebLibrary.Models
{
    public class JsonDataBaseDocument
    {
        public string type { get; set; } = "database_document";
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
