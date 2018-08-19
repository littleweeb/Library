using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LittleWeebLibrary.Models
{
    public class JsonLittleWeebSettings
    {

        public string type = "littleweeb_settings";
        public int port { get; set; } = -1;
        public bool local { get; set; } = false;
        public string version { get; set; } = string.Empty;
        public int randomusernamelength { get; set; } = -1;
        public int maxdebuglogsize { get; set; } = -1;
        public List<int> debuglevel { get; set; } = new List<int>();
        public List<int> debugtype { get; set; } = new List<int>();


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
