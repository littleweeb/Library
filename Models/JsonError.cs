using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    public class JsonError
    {
        public string type = "error";
        public string errortype { get; set; } = string.Empty;
        public string errormessage { get; set; } = string.Empty;
        public string exception { get; set; } = string.Empty;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            JObject jobject = JObject.FromObject(this);
            string properties = string.Empty;
            foreach (var x in jobject)
            {
                properties += x.Key + ": " + x.Value.ToString();
            }
            return properties;
        }
    }
}
