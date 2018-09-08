using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Models
{
    public class JsonVersionInfo
    {
        public string type = "version_update";
        public bool update_available { get; set; } = false;
        public string currentversion { get; set; } = "Not Set";
        public string currentbuild { get; set; } = "Not Set";
        public string newversion { get; set; } = "Not Found";
        public string newbuild { get; set; } = "Not Found";
        public string release_url { get; set; } = "Not Found";
        public string direct_download_url { get; set; } = "Not Found";
        public string file_name { get; set; } = "Not Found";
        public string date { get; set; } = "0000-00-00T00:00:00Z";
        public string release_version { get; set; } = "develop"; 
    }
}
