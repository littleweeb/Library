using LittleWeebLibrary.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.GlobalInterfaces
{
    interface ISettingsInterface
    {
        void SetIrcSettings(IrcSettings settings);
        void SetLittleWeebSettings(LittleWeebSettings settings);
    }
}
