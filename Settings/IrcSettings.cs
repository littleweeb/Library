using System;
using System.IO;

namespace LittleWeebLibrary.Settings
{
    public class IrcSettings
    {
        public string ServerAddress { get; set; } = "irc.rizon.net";
        public int Port { get; set; } = 6697;
        public bool Secure { get; set; } = true;
        public string Channels { get; set; } = "#nibl,#horriblesubs,#news";
        public string UserName { get; set; } = string.Empty;

#if __ANDROID__
        public string fullfilepath  {get;set;}=  Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LittleWeeb"), "Downloads");
#else
        public string fullfilepath{ get; set; } = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LittleWeeb"), "Downloads");
#endif

        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "ServerAddress: " + ServerAddress + Environment.NewLine;
            toReturn += "Port: " + Port.ToString() + Environment.NewLine;
            toReturn += "Secure: " + Secure.ToString() + Environment.NewLine;
            toReturn += "Channels: " + Channels.ToString() + Environment.NewLine;
            toReturn += "UserName: " + UserName + Environment.NewLine;
            toReturn += "fullfilepath: " + fullfilepath+ Environment.NewLine;
            return toReturn;
        }
    }
}
