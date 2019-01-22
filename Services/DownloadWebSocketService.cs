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
                JsonDownloadInfo downloadInfo = new JsonDownloadInfo()
                {
                    animeInfo = new JsonAnimeInfo()
                    {
                        anime_id = downloadJson.Value<JObject>("animeInfo").Value<string>("animeid"),
                        anime_title = downloadJson.Value<JObject>("animeInfo").Value<string>("title"),
                        anime_cover_original = downloadJson.Value<JObject>("animeInfo").Value<string>("cover_original"),
                        anime_cover_small = downloadJson.Value<JObject>("animeInfo").Value<string>("cover_small")
                    },
                    id = downloadJson.Value<string>("id"),
                    episodeNumber = downloadJson.Value<string>("episodeNumber"),
                    pack = downloadJson.Value<string>("pack"),
                    bot = downloadJson.Value<string>("bot"),
                    fullfilepath= downloadJson.Value<string>("dir"),
                    filename = downloadJson.Value<string>("filename"),
                    progress = downloadJson.Value<string>("progress"),
                    speed = downloadJson.Value<string>("speed"),
                    status = downloadJson.Value<string>("status"),
                    filesize = downloadJson.Value<string>("filesize")
                };

                LastDownloadedInfo = downloadInfo;

                string result = DownloadHandler.AddDownload(downloadInfo);

                await WebSocketHandler.SendMessage(result);
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
                string result = "";
                if (id != null)
                {

                    result = DownloadHandler.RemoveDownload(id, null);
                    await WebSocketHandler.SendMessage(result);

                    string toRemove = FileHistoryHandler.RemoveFileFromFileHistory(id, null);
                    string resultRemove = FileHandler.DeleteFile(toRemove);
                    await  WebSocketHandler.SendMessage(resultRemove);
                }
                else if (filePath != null)
                {
                    result = DownloadHandler.RemoveDownload(null, filePath);
                    await WebSocketHandler.SendMessage(result);
                    string toRemove = FileHistoryHandler.RemoveFileFromFileHistory(null, filePath);
                    string resultRemove = FileHandler.DeleteFile(toRemove);
                    await WebSocketHandler.SendMessage(resultRemove);
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
            await WebSocketHandler.SendMessage(FileHistoryHandler.GetCurrentFileHistory().ToJson());
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
                JsonDownloadInfo update = new JsonDownloadInfo()
                {
                    id = args.id,
                    animeInfo = new JsonAnimeInfo()
                    {
                        anime_cover_original = args.animeCoverOriginal,
                        anime_id = args.animeid,
                        anime_cover_small = args.animeCoverSmall,
                        anime_title = args.animeTitle
                    },
                    episodeNumber = args.episodeNumber,
                    bot = args.bot,
                    pack = args.pack,
                    progress = args.progress,
                    speed = args.speed,
                    status = args.status,
                    filename = args.filename,
                    filesize = args.filesize,
                    downloadIndex = args.downloadIndex,
                    fullfilepath= args.fullfilepath
                };

                await WebSocketHandler.SendMessage(update.ToJson());
                if (update.filename != null && update.fullfilepath!= null)
                {
                    FileHistoryHandler.AddFileToFileHistory(update);
                }

                if (update.status == "FAILED" || update.status == "ABORTED")
                {
                    FileHistoryHandler.RemoveFileFromFileHistory(update.id);
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

                string result = await DownloadHandler.AbortDownload(id);
                await WebSocketHandler.SendMessage(result);
            }
            else if (filePath != null)
            {
                string result = await DownloadHandler.AbortDownload(null, filePath);
                await WebSocketHandler.SendMessage(result);
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
                        animeInfo = new JsonAnimeInfo()
                        {
                            anime_id = downloadJson.Value<JObject>("animeInfo").Value<string>("animeid"),
                            anime_title = downloadJson.Value<JObject>("animeInfo").Value<string>("title"),
                            anime_cover_original = downloadJson.Value<JObject>("animeInfo").Value<string>("cover_original"),
                            anime_cover_small = downloadJson.Value<JObject>("animeInfo").Value<string>("cover_small")
                        },
                        id = downloadJson.Value<string>("id"),
                        episodeNumber = downloadJson.Value<string>("episodeNumber"),
                        pack = downloadJson.Value<string>("pack"),
                        bot = downloadJson.Value<string>("bot"),
                        fullfilepath = downloadJson.Value<string>("dir"),
                        filename = downloadJson.Value<string>("filename"),
                        progress = downloadJson.Value<string>("progress"),
                        speed = downloadJson.Value<string>("speed"),
                        status = downloadJson.Value<string>("status"),
                        filesize = downloadJson.Value<string>("filesize")
                    };

                    LastDownloadedInfo = downloadInfo;
                    batch.Add(downloadInfo);
                }
                
                string result = DownloadHandler.AddDownloads(batch);

                await WebSocketHandler.SendMessage(result);
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
