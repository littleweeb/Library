using LittleWeebLibrary.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.EventArguments
{
    public class IrcClientConnectionStatusArgs
    {
        public bool Connected { get; set; } = false;
        public Dictionary<string, List<string>> ChannelsAndUsers { get; set; } = new Dictionary<string, List<string>>();
        public IrcSettings CurrentIrcSettings { get; set; } = new IrcSettings();

        public override string ToString()
        {
            string toReturn = string.Empty;

            toReturn += "Connected: " + Connected.ToString() + Environment.NewLine;
            toReturn += "ChannelsAndUsers: " + ChannelsAndUsers.ToString() + Environment.NewLine;
            toReturn += "Keys and it Values:";
            foreach (KeyValuePair<string, List<string>> data in ChannelsAndUsers)
            {
                toReturn += "Key: " + data.Key + Environment.NewLine;
                foreach (string value in data.Value)
                {
                    toReturn += "Value: " + value + Environment.NewLine;
                }
            }
            toReturn += CurrentIrcSettings.ToString();
            return toReturn;
        }
    }
}
