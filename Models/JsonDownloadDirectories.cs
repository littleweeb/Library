using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    class JsonDownloadDirectories
    {
        public string type = "downloaded_directories"; //used for identifying json
        public JsonFreeSpace freeSpace { get; set; } = new JsonFreeSpace();
        public List<Jsonfullfilepath> directories { get; set; } = new List<Jsonfullfilepath>();

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
