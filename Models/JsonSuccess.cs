using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LittleWeebLibrary.Models
{
    public class JsonSuccess
    {

        public string type = "success";
        public string message { get; set; } = string.Empty;
        public string updated { get; set; } = StaticClasses.UtilityMethods.GetEpoch().ToString();

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
