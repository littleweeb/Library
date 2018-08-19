using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LittleWeebLibrary.Settings
{
    public class LittleWeebSettings
    {
        public int Port { get; set; }
        public bool Local { get; set; } 
        public string Version { get; set; }
        public int RandomUsernameLength { get; set; }
        public List<int> DebugLevel { get; set; }
        public List<int> DebugType { get; set; }
        public int MaxDebugLogSize { get; set; }

        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "Port: " + Port.ToString() + Environment.NewLine;
            toReturn += "Local: " + Local.ToString() + Environment.NewLine;
            toReturn += "Version: " + Version + Environment.NewLine;
            toReturn += "RandomUserNameLength:: " + RandomUsernameLength.ToString() + Environment.NewLine;
            toReturn += "DebugLevel: ";
            foreach (int debugLevel in DebugLevel) {
                toReturn += debugLevel.ToString() + ",";
            }
            toReturn += "DebugLevel: ";
            foreach (int debugType in DebugType)
            {
                toReturn += debugType.ToString() + ",";
            }
            toReturn += Environment.NewLine;
            toReturn += "MaxDebugLogSize: " + MaxDebugLogSize.ToString() + Environment.NewLine;

            return toReturn;
        }
    }
}
