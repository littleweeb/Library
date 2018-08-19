using LittleWeebLibrary.EventArguments;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.GlobalInterfaces
{
    public interface IDebugEvent
    {
        event EventHandler<BaseDebugArgs> OnDebugEvent;
    }
}
