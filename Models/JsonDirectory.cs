using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LittleWeebLibrary.Models
{
    class JsonDirectory
    {
        public string type = "directory";
        public string path { get; set; } = string.Empty;
        public string dirname { get; set; } = string.Empty;

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
