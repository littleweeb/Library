using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Services
{
    public interface ISettingsWebSocketService
    {
        Task GetCurrentIrcSettings();
        Task GetCurrentLittleWeebSettings();
        Task SetIrcSettings(JObject jsonIrcSettings);
        Task SetLittleWeebSettings(JObject jsonLittleWeebSettings);
        Task Setfullfilepath(JObject fullfilepathJson);
        Task SetSettingsClasses(
            ISettingsHandler settingsHandler,
            IIrcClientHandler ircClientHandler,
            IFileHandler fileHandler,
            IDownloadHandler downloadHandler,
            IDirectoryWebSocketService directoryWebSocketService,
            IIrcWebSocketService ircWebSocketService
            );
    }
    public class SettingsWebSocketService : ISettingsWebSocketService
    {
       


        private IDebugHandler DebugHandler;
        private IWebSocketHandler WebSocketHandler;
        private IIrcClientHandler IrcClientHandler;
        private ISettingsHandler SettingsHandler;
        private IDirectoryHandler DirectoryHandler;
        private IDownloadHandler DownloadHandler;
        private IDirectoryWebSocketService DirectoryWebSocketService;
        private IIrcWebSocketService IrcWebSocketService;
        private ISettingsInterface WebSocketHandlerSettings;
        private ISettingsInterface IrcClientHandlerSettings;
        private ISettingsInterface DebugHandlerSettings;
        private ISettingsInterface FileHandlerSettings;
        private ISettingsInterface DownloadHandlerSettings;
        private ISettingsInterface DirectoryWebSocketServiceSettings;
        private ISettingsInterface IrcWebSocketServiceSettings;
        private LittleWeebSettings LittleWeebSettings;
        private IrcSettings IrcSettings;

        public SettingsWebSocketService(IWebSocketHandler webSocketHandler,
            IDirectoryHandler directoryHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            WebSocketHandler = webSocketHandler;
            DirectoryHandler = directoryHandler;
            DebugHandler = debugHandler;

        }

        public Task SetSettingsClasses(
            ISettingsHandler settingsHandler,
            IIrcClientHandler ircClientHandler,
            IFileHandler fileHandler,
            IDownloadHandler downloadHandler,
            IDirectoryWebSocketService directoryWebSocketService,
            IIrcWebSocketService ircWebSocketService
            )
        {


            DebugHandler.TraceMessage("SetSettingsClasses Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            SettingsHandler = settingsHandler;
            IrcClientHandler = ircClientHandler;
            DownloadHandler = downloadHandler;
            DirectoryWebSocketService = directoryWebSocketService;
            IrcWebSocketService = ircWebSocketService;


            WebSocketHandlerSettings = WebSocketHandler as ISettingsInterface;
            IrcClientHandlerSettings = ircClientHandler as ISettingsInterface;
            DebugHandlerSettings = DebugHandler as ISettingsInterface;
            FileHandlerSettings = fileHandler as ISettingsInterface;
            DownloadHandlerSettings = downloadHandler as ISettingsInterface;
            DirectoryWebSocketServiceSettings = directoryWebSocketService as ISettingsInterface;
            IrcWebSocketServiceSettings = ircWebSocketService as ISettingsInterface;

            LittleWeebSettings = settingsHandler.GetLittleWeebSettings();
            IrcSettings = settingsHandler.GetIrcSettings();

            SetAllIrcSettings(IrcSettings);
            SetAllLittleWeebSettings(LittleWeebSettings);

            return Task.CompletedTask;
        }

        public async Task GetCurrentIrcSettings()
        {

            DebugHandler.TraceMessage("GetCurrentIrcSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            IrcSettings = SettingsHandler.GetIrcSettings();
            JsonIrcInfo info = new JsonIrcInfo()
            {
                channel = IrcSettings.Channels,
                server = IrcSettings.ServerAddress,
                user = IrcSettings.UserName,
                fullfilepath= IrcSettings.fullfilepath
            };

            await WebSocketHandler.SendMessage(info.ToJson());

        }

        public async Task GetCurrentLittleWeebSettings()
        {
            DebugHandler.TraceMessage("GetCurrentLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            LittleWeebSettings = SettingsHandler.GetLittleWeebSettings();

            JsonLittleWeebSettings settings = new JsonLittleWeebSettings()
            {
                port = LittleWeebSettings.Port,
                local = LittleWeebSettings.Local,
                randomusernamelength = LittleWeebSettings.RandomUsernameLength,
                debuglevel = LittleWeebSettings.DebugLevel,
                debugtype = LittleWeebSettings.DebugType,
                maxdebuglogsize = LittleWeebSettings.MaxDebugLogSize
            };
            await WebSocketHandler.SendMessage(settings.ToJson());
        }

        public async Task SetIrcSettings(JObject jsonIrcSettings)
        {
            DebugHandler.TraceMessage("SetIrcSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(jsonIrcSettings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                IrcSettings = SettingsHandler.GetIrcSettings();

                IrcSettings.ServerAddress = jsonIrcSettings.Value<string>("address");
                IrcSettings.Channels = jsonIrcSettings.Value<string>("channels");
                IrcSettings.UserName = jsonIrcSettings.Value<string>("username");
                IrcSettings.fullfilepath= jsonIrcSettings.Value<string>("fullfilepath");
                IrcSettings.Port = jsonIrcSettings.Value<int>("port");
                IrcSettings.Secure = jsonIrcSettings.Value<bool>("secure");

                SetAllIrcSettings(IrcSettings);
                SettingsHandler.WriteIrcSettings(IrcSettings);
                await GetCurrentIrcSettings();
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "set_irc_settings_error",
                    errortype = "Exception",
                    errormessage = "Failed to set irc settings.",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }
        }

        public async Task SetLittleWeebSettings(JObject jsonLittleWeebSettings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(jsonLittleWeebSettings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                LittleWeebSettings = SettingsHandler.GetLittleWeebSettings();

                LittleWeebSettings.RandomUsernameLength = jsonLittleWeebSettings.Value<int>("randomusernamelength");
                LittleWeebSettings.DebugLevel = jsonLittleWeebSettings.Value<JArray>("debuglevel").ToObject<List<int>>();
                LittleWeebSettings.DebugType = jsonLittleWeebSettings.Value<JArray>("debugtype").ToObject<List<int>>();
                LittleWeebSettings.MaxDebugLogSize = jsonLittleWeebSettings.Value<int>("maxdebuglogsize");
                SetAllLittleWeebSettings(LittleWeebSettings);


                SettingsHandler.WriteLittleWeebSettings(LittleWeebSettings);

                await GetCurrentLittleWeebSettings();
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
                JsonError error = new JsonError
                {
                    type = "set_littleweeb_settings_error",
                    errortype = "Exception",
                    errormessage = "Failed to set littleweeb settings.",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task Setfullfilepath(JObject fullfilepathJson)
        {
            DebugHandler.TraceMessage("Setfullfilepath Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(fullfilepathJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            try
            {
                string path = fullfilepathJson.Value<string>("path");
                DirectoryHandler.CreateDirectory(path, "");
                IrcSettings.fullfilepath= path;
                SetAllIrcSettings(IrcSettings);
                SettingsHandler.WriteIrcSettings(IrcSettings);
                await GetCurrentIrcSettings();
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError jsonError = new JsonError
                {
                    type = "set_download_directory_error",
                    errortype = "Exception",
                    errormessage = "Failed to set custom download directory.",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }

        }

        private void SetAllLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetAllLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcClientHandlerSettings.SetLittleWeebSettings(settings);
            FileHandlerSettings.SetLittleWeebSettings(settings);
            DownloadHandlerSettings.SetLittleWeebSettings(settings);
            DirectoryWebSocketServiceSettings.SetLittleWeebSettings(settings);
            IrcWebSocketServiceSettings.SetLittleWeebSettings(settings);
        }

        private void SetAllIrcSettings(IrcSettings settings)
        {
            DebugHandler.TraceMessage("SetAllIrcSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcClientHandlerSettings.SetIrcSettings(settings);
            FileHandlerSettings.SetIrcSettings(settings);
            DownloadHandlerSettings.SetIrcSettings(settings);
            DirectoryWebSocketServiceSettings.SetIrcSettings(settings);
            IrcWebSocketServiceSettings.SetIrcSettings(settings);
        }
    }
}
