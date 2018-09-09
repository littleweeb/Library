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

            try
            {

                if (versionInfoDevelop.newversion != "Not Found" && currentVersion.currentversion != "Not Found")
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Current: " + currentVersion.ToJson(),
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Develop: " + versionInfoDevelop.ToJson(),
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    int currentBuild = int.Parse(currentVersion.currentbuild);
                    int newBuild = int.Parse(versionInfoDevelop.newbuild);

                    if (versionInfoDevelop.newversion == currentVersion.currentversion && currentBuild >= newBuild)
                    {
                        versionInfoDevelop.update_available = false;
                    }
                    else
                    {
                        versionInfoDevelop.update_available = true;
                    }

                }

                if (versionInfoRelease.newversion != "Not Found" && currentVersion.currentversion != "Not Found")
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Current: " + currentVersion.ToJson(),
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugMessage = "Release: " + versionInfoRelease.ToJson(),
                        DebugSource = this.GetType().Name,
                        DebugSourceType = 1,
                        DebugType = 2
                    });
                    int currentBuild = int.Parse(currentVersion.currentbuild);
                    int newBuild = int.Parse(versionInfoRelease.newbuild);
                    if (versionInfoRelease.newversion == currentVersion.currentversion && currentBuild >= newBuild)
                    {
                        versionInfoRelease.update_available = false;
                    }
                    else
                    {
                        versionInfoRelease.update_available = true;
                    }

                }

                versionInfoDevelop.currentbuild = currentVersion.currentbuild;
                versionInfoDevelop.currentversion = currentVersion.currentversion;

                await WebSocketHandler.SendMessage(versionInfoDevelop.ToJson());

                versionInfoRelease.currentbuild = currentVersion.currentbuild;
                versionInfoRelease.currentversion = currentVersion.currentversion;

                await WebSocketHandler.SendMessage(versionInfoRelease.ToJson());
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugMessage = e.ToString(),
                    DebugSource = this.GetType().Name,
                    DebugSourceType = 1,
                    DebugType = 4
                });
            }


        }
    }
}
