using LittleWeebLibrary.Settings;

namespace LittleWeebLibrary.GlobalInterfaces
{
    interface ISettingsInterface
    {
        void SetIrcSettings(IrcSettings settings);
        void SetLittleWeebSettings(LittleWeebSettings settings);
    }
}
