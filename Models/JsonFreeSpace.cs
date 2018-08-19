using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    class JsonFreeSpace
    {
        public string type = "free_space";
        public long freespacebytes { get; set; } = -1;
        public long freespacekbytes { get; set; } = -1;
        public long freespacembytes { get; set; } = -1;

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
