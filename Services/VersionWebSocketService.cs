using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Services
{
    public interface IVersionWebSocketService
    {
        Task CheckVersion();
    }

    public class VersionWebSocketService : IVersionWebSocketService, IDebugEvent
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private IVersionHandler VersionHandler;
        private IWebSocketHandler WebSocketHandler;

        public VersionWebSocketService(IWebSocketHandler webSocketHandler, IVersionHandler versionHandler)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "Constructor called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 0,
                DebugType = 0
            });

            VersionHandler = versionHandler;
            WebSocketHandler = webSocketHandler;
        }

        public async Task CheckVersion()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "CheckVersion called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            JsonVersionInfo versionInfoDevelop = new JsonVersionInfo();
            JsonVersionInfo versionInfoRelease = new JsonVersionInfo();


            versionInfoDevelop = await VersionHandler.GetLatestVersionDesktop(false);
            versionInfoRelease = await VersionHandler.GetLatestVersionDesktop(true);



#if __ANDROID__
            versionInfoDevelop = await VersionHandler.GetLatestVersionAndroid(false);
            versionInfoRelease = await VersionHandler.GetLatestVersionAndroid(true);
#endif

            JsonVersionInfo currentVersion = VersionHandler.GetLocalVersion();

            if (versionInfoDevelop.newversion == currentVersion.currentversion && versionInfoDevelop.newbuild == currentVersion.currentbuild)
            {
                versionInfoDevelop.update_available = false;
            }
            else if( versionInfoDevelop.newversion != "Not Found")
            {
                versionInfoDevelop.update_available = true;
            }

            if (versionInfoRelease.newversion == currentVersion.currentversion && versionInfoRelease.newbuild == currentVersion.currentbuild )
            {
                versionInfoRelease.update_available = false;
            }
            else if(versionInfoRelease.newversion != "Not Found")
            {
                versionInfoRelease.update_available = true;
            }

            versionInfoDevelop.currentbuild = currentVersion.currentbuild;
            versionInfoDevelop.currentversion = currentVersion.currentversion;

            await WebSocketHandler.SendMessage(JsonConvert.SerializeObject(versionInfoDevelop));

            versionInfoRelease.currentbuild = currentVersion.currentbuild;
            versionInfoRelease.currentversion = currentVersion.currentversion;

            await WebSocketHandler.SendMessage(JsonConvert.SerializeObject(versionInfoRelease));

        }
    }
}
