using LittleWeebLibrary.EventArguments;
using System;

namespace LittleWeebLibrary.GlobalInterfaces
{
    public interface IDebugEvent
    {
        event EventHandler<BaseDebugArgs> OnDebugEvent;
    }
}
