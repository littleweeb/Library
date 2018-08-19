using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.EventArguments
{
    public class WebSocketEventArgs
    {
        public string Message { get; set; }
        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "Message: " + Message + Environment.NewLine;
            return toReturn;
        }
    }
}
