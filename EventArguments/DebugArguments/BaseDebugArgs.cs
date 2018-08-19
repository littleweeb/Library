using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.EventArguments
{
    public class BaseDebugArgs
    {
        public string DebugMessage { get; set; }
        public string DebugSource { get; set; }
        public int DebugSourceType { get; set; } = 99;// 0 = constructor, 1 = method, 2 = event, 3 = task, 4 = external(library), 99 = undefined.

        // https://stackoverflow.com/questions/312378/debug-levels-when-writing-an-application
        public int DebugType { get; set; } // empty || null = none, 0 = entry/exit, 1 = parameters, 2 = info, 3 = warning, 4 = error, 5 = servere, > 5 = none

        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "DebugMessage: " + DebugMessage + Environment.NewLine;
            toReturn += "DebugSource: " + DebugSource + Environment.NewLine;
            toReturn += "DebugType: " + DebugType + Environment.NewLine;
            return toReturn;
        }
    }
}
