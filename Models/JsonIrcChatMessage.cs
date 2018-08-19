using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    class JsonIrcChatMessage
    {
        public string type = "chat_message"; //used for identifying json
        public string channel { get; set; } = string.Empty;
        public string user { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;

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
