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
    public interface IDownloadWebSocketService
    {
        Task AddDownload(JObject downloadJson);
        Task AddDownloads(JObject downloadJsonBatch);
        Task AbortDownload(JObject downloadJson);
        Task RemoveDownload(JObject downloadJson);
        Task Openfullfilepath();
        Task GetCurrentFileHistory();
    }
    public class DownloadWebSocketService : IDownloadWebSocketService, ISettingsInterface
    {

        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDirectoryHandler DirectoryHandler;
        private readonly IDownloadHandler DownloadHandler;
        private readonly IFileHistoryHandler FileHistoryHandler;
        private readonly IFileHandler FileHandler;
        private readonly ISettingsHandler SettingsHandler;
        private readonly IDebugHandler DebugHandler;
        private LittleWeebSettings LittleWeebSettings;
        private IrcSettings IrcSettings;

        private JsonDownloadInfo LastDownloadedInfo;

       

        public DownloadWebSocketService(
            IWebSocketHandler webSocketHandler,
            IDirectoryHandler directoryHandler,
            IDownloadHandler downloadHandler,
            IFileHandler fileHandler,
            IFileHistoryHandler fileHistoryHandler,
            ISettingsHandler settingsHandler,
            IDebugHandler debugHandler)
        {
            

            debugHandler.TraceMessage("Constructor called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            DebugHandler = debugHandler;
            WebSocketHandler = webSocketHandler;
            DirectoryHandler = directoryHandler;
            DownloadHandler = downloadHandler;
            FileHandler = fileHandler;
            FileHistoryHandler = fileHistoryHandler;
            SettingsHandler = settingsHandler;
            LastDownloadedInfo = new JsonDownloadInfo();

            LittleWeebSettings = SettingsHandler.GetLittleWeebSettings();
            IrcSettings = SettingsHandler.GetIrcSettings();


            downloadHandler.OnDownloadUpdateEvent += OnDownloadUpdateEvent;
        }

        public async Task AddDownload(JObject downloadJson)
        {

            DebugHandler.TraceMessage("AddDownload called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
                    
            try
            {
                JsonDownloadInfo downloadInfo = downloadJson.ToObject<JsonDownloadInfo>();

                LastDownloadedInfo = downloadInfo;

                JObject result = await DownloadHandler.AddDownload(downloadInfo);

                await WebSocketHandler.SendMessage(result.ToString());
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
              
                JsonError error = new JsonError()
                {
                    type = "parse_download_to_add_error",
                    errormessage = "Could not parse json containing download to add information.",
                    errortype = "exception",
                    exception = e.ToString()
                };


                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task RemoveDownload(JObject downloadJson)
        {

            DebugHandler.TraceMessage("RemoveDownload called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                string id = downloadJson.Value<string>("id");
                string filePath = downloadJson.Value<string>("path");
                JObject result = new JObject();
                if (id != null)
                {
                    result = await DownloadHandler.RemoveDownload(id);
                    await WebSocketHandler.SendMessage(result.ToString());
                }
                else
                {
                    JsonError error = new JsonError()
                    {
                        type = "parse_download_to_remove_error",
                        errormessage = "Neither id or file path have been defined!",
                        errortype = "warning",
                        exception = "none"
                    };
                    await WebSocketHandler.SendMessage(error.ToJson());
                }


            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError error = new JsonError()
                {
                    type = "parse_download_to_remove_error",
                    errormessage = "Could not parse json containing download to remove information.",
                    errortype = "exception",
                    exception = e.ToString()
                };


                await WebSocketHandler.SendMessage(error.ToJson());
            }

        }

        public async Task GetCurrentFileHistory()
        {
            DebugHandler.TraceMessage("GetCurrentFileHistory called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            JsonDownloadedList info = await FileHistoryHandler.GetCurrentFileHistory();
            await WebSocketHandler.SendMessage(info.ToJObject().ToString());
        }

        public async Task  Openfullfilepath()
        {
            DebugHandler.TraceMessage("Openfullfilepath called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            string result = DirectoryHandler.OpenDirectory(IrcSettings.fullfilepath);
            await WebSocketHandler.SendMessage(result);
        }

        private async void OnDownloadUpdateEvent(object sender, DownloadUpdateEventArgs args)
        {
            DebugHandler.TraceMessage("OnDownloadUpdateEvent called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                await WebSocketHandler.SendMessage(args.downloadInfo.ToJson());

                if (args.downloadInfo.status == "COMPLETED")
                {
                    await GetCurrentFileHistory();
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            DebugHandler.TraceMessage("SetIrcSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            LittleWeebSettings = settings;
        }

        public async Task AbortDownload(JObject downloadJson)
        {
            DebugHandler.TraceMessage("AbortDownload called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            string id = downloadJson.Value<string>("id");
            string filePath = downloadJson.Value<string>("path");
            if (id != null)
            {

                JObject result = await DownloadHandler.AbortDownload(id);
                await WebSocketHandler.SendMessage(result.ToString());
            }
            else
            {
                DebugHandler.TraceMessage("Neither id or file path have been defined!", DebugSource.TASK, DebugType.WARNING);
                JsonError error = new JsonError()
                {
                    type = "parse_download_to_abort_error",
                    errormessage = "Neither id or file path have been defined!",
                    errortype = "warning",
                    exception = "none"
                };
                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public async Task AddDownloads(JObject downloadJsonBatch)
        {
            DebugHandler.TraceMessage("AddDownloads called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(downloadJsonBatch.ToString(), DebugSource.TASK, DebugType.PARAMETERS);         

            try
            {

                JArray listWithDownloads = downloadJsonBatch.Value<JArray>("batch");

                List<JsonDownloadInfo> batch = new List<JsonDownloadInfo>();

                foreach (JObject downloadJson in listWithDownloads)
                {
                    JsonDownloadInfo downloadInfo = new JsonDownloadInfo()
                    {

                        anime_id = downloadJson.Value<JObject>("animeInfo").Value<string>("animeid"),
                        anime_name = downloadJson.Value<JObject>("animeInfo").Value<string>("title"),
                        id = downloadJson.Value<string>("id"),
                        episodeNumber = downloadJson.Value<string>("episodeNumber"),
                        season = downloadJson.Value<string>("season"),
                        pack = downloadJson.Value<string>("pack"),
                        bot = downloadJson.Value<string>("bot"),
                        filename = downloadJson.Value<string>("filename"),
                        progress = downloadJson.Value<string>("progress"),
                        speed = downloadJson.Value<string>("speed"),
                        status = downloadJson.Value<string>("status"),
                        filesize = downloadJson.Value<string>("filesize")
                    };

                    LastDownloadedInfo = downloadInfo;
                    batch.Add(downloadInfo);
                }
                
                JObject result = await DownloadHandler.AddDownloads(batch);

                await WebSocketHandler.SendMessage(result.ToString());
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonError error = new JsonError()
                {
                    type = "parse_downloads_to_add_error",
                    errormessage = "Could not parse json containing downloads to add information.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }
    }
}
