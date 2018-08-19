using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.EventArguments
{
    public class IrcClientMessageEventArgs
    {
        public string Message { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "Message: " + Message + Environment.NewLine;
            toReturn += "User: " + User + Environment.NewLine;
            toReturn += "Channel: " + Channel + Environment.NewLine;
            return toReturn;
        }
    }
}
