using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.StaticClasses;
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

    public class VersionWebSocketService : IVersionWebSocketService
    {

        private IVersionHandler VersionHandler;
        private IWebSocketHandler WebSocketHandler;
        private IDebugHandler DebugHandler;

        public VersionWebSocketService(IWebSocketHandler webSocketHandler, IVersionHandler versionHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            VersionHandler = versionHandler;
            WebSocketHandler = webSocketHandler;
            DebugHandler = debugHandler;
        }

        public async Task CheckVersion()
        {
            DebugHandler.TraceMessage("CheckVersion called", DebugSource.TASK, DebugType.ENTRY_EXIT);
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


                    DebugHandler.TraceMessage("Current: " + currentVersion.ToJson(), DebugSource.TASK, DebugType.INFO);
                    DebugHandler.TraceMessage("Develop: " + versionInfoDevelop.ToJson(), DebugSource.TASK, DebugType.INFO);

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

                    DebugHandler.TraceMessage("Current: " + currentVersion.ToJson(), DebugSource.TASK, DebugType.INFO);
                    DebugHandler.TraceMessage("Develop: " + versionInfoRelease.ToJson(), DebugSource.TASK, DebugType.INFO);
                    
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
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }
    }
}
